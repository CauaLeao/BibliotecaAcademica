using BibliotecaMVC.Enums;
using BibliotecaMVC.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BibliotecaMVC.Filters;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class AutorizarOperacaoAttribute : TypeFilterAttribute
{
    public AutorizarOperacaoAttribute(TipoOperacao operacao)
        : base(typeof(AutorizarOperacaoFilter))
    {
        Arguments = [operacao];
    }

    private class AutorizarOperacaoFilter : IAsyncActionFilter
    {
        private readonly TipoOperacao _operacao;
        private readonly IUsuarioRepository _usuarioRepository;

        public AutorizarOperacaoFilter(TipoOperacao operacao, IUsuarioRepository usuarioRepository)
        {
            _operacao = operacao;
            _usuarioRepository = usuarioRepository;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var session = context.HttpContext.Session;
            var usuarioId = session.GetInt32("UsuarioId");

            // 1. usuário não logado >> manda para o Login guardando a URL que tentou acessar
            if (!usuarioId.HasValue)
            {
                var returnUrl = context.HttpContext.Request.Path + context.HttpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl });
                return;
            }

            // 2. Busca o usuário no banco (já reconstituído como Cliente, Bibliotecario ou Administrador)
            var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId.Value);

            // Se foi excluído ou desativado depois que logou, derruba a sessão (RN03)
            if (usuario is null || !usuario.CadastroAtivo)
            {
                session.Clear();
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // 3. Polimorfismo em ação: cada subclasse decide se permite a operação
            if (!usuario.PossuiPermissao(_operacao))
            {
                context.Result = new RedirectToActionResult("AcessoNegado", "Account", null);
                return;
            }

            // 4. Tudo certo → deixa a Action do Controller rodar
            await next();
        }
    }
}