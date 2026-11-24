using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Filters;
using BibliotecaMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

public class EmprestimosController : Controller
{
    private readonly EmprestimoService _emprestimoService;
    private readonly LivroService _livroService;
    private readonly UsuarioService _usuarioService;

    public EmprestimosController(
        EmprestimoService emprestimoService,
        LivroService livroService,
        UsuarioService usuarioService)
    {
        _emprestimoService = emprestimoService;
        _livroService = livroService;
        _usuarioService = usuarioService;
    }

    // Visão do Leitor: Seus próprios empréstimos
    [HttpGet]
    [AutorizarOperacao(TipoOperacao.ConsultarPropriosEmprestimos)]
    public async Task<IActionResult> Meus()
    {
        var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;
        var emprestimos = await _emprestimoService.ListarPorUsuarioAsync(usuarioId); // Lista todos os empréstimos do usuário logado (ativos e devolvidos)
        return View(emprestimos);
    }

    // Visão do Gestor: Todos os empréstimos 
    [HttpGet]
    [AutorizarOperacao(TipoOperacao.ConsultarTodosEmprestimos)]
    public async Task<IActionResult> Index()
    {
        var emprestimos = await _emprestimoService.ListarTodosAsync(); // Lista todos os empréstimos do sistema (ativos e devolvidos)
        return View(emprestimos);
    }

    // Renovar empréstimo
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AutorizarOperacao(TipoOperacao.ConsultarPropriosEmprestimos)]
    public async Task<IActionResult> Renovar(int id)
    {
        try
        {
            await _emprestimoService.RenovarAsync(id); // Puxa o serviço de domínio para renovar o empréstimo
            TempData["Sucesso"] = "Empréstimo renovado com sucesso!";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        var tipo = HttpContext.Session.GetString("TipoUsuario");
        return (tipo == "Bibliotecario" || tipo == "Administrador")
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Meus));
    }

    // Devolução de exemplar (Apenas Bibliotecário / Admin)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AutorizarOperacao(TipoOperacao.RegistrarDevolucao)] // Bloqueia o acesso de usuários que não possuem permissão para registrar devoluções
    public async Task<IActionResult> Devolver(int id, EstadoConservacao estadoConservacao)
    {
        try
        {
            await _emprestimoService.RegistrarDevolucaoAsync(id, estadoConservacao); // Puxa o serviço de domínio para registrar a devolução do exemplar
            TempData["Sucesso"] = "Devolução registrada e exemplar atualizado com sucesso!";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}