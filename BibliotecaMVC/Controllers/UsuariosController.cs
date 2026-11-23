using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Filters;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;
using BibliotecaMVC.Services;
using BibliotecaMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

[AutorizarOperacao(TipoOperacao.GerenciarClientes)]
public class UsuariosController : Controller
{
    private readonly UsuarioService _usuarioService;
    private readonly IEmprestimoRepository _emprestimoRepository;

    public UsuariosController(UsuarioService usuarioService, IEmprestimoRepository emprestimoRepository)
    {
        _usuarioService = usuarioService;
        _emprestimoRepository = emprestimoRepository;
    }

    private async Task<Usuario> ObterOperadorLogadoAsync()
    {
        var usuarioId = HttpContext.Session.GetInt32("UsuarioId")
            ?? throw new RegraNegocioException("Sessão expirada. Faça login novamente.");

        return await _usuarioService.ObterPorIdAsync(usuarioId);
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var operador = await ObterOperadorLogadoAsync();
        var usuarios = await _usuarioService.ListarParaOperadorAsync(operador);
        ViewBag.IsAdmin = operador.PossuiPermissao(TipoOperacao.GerenciarBibliotecarios);
        return View(usuarios);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var operador = await ObterOperadorLogadoAsync();
            var alvo = await _usuarioService.ObterPorIdAsync(id);

            if (alvo is not Cliente && !operador.PossuiPermissao(TipoOperacao.GerenciarBibliotecarios))
                return RedirectToAction("AcessoNegado", "Account");

            var emprestimos = await _emprestimoRepository.ObterPorUsuarioIdAsync(id);

            var viewModel = new UsuarioDetalhesViewModel
            {
                Usuario = alvo,
                Emprestimos = emprestimos
            };

            return View(viewModel);
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var operador = await ObterOperadorLogadoAsync();
        var model = new UsuarioCriarViewModel
        {
            PodeCriarBibliotecario = operador.PossuiPermissao(TipoOperacao.GerenciarBibliotecarios)
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UsuarioCriarViewModel model)
    {
        var operador = await ObterOperadorLogadoAsync();
        model.PodeCriarBibliotecario = operador.PossuiPermissao(TipoOperacao.GerenciarBibliotecarios);

        if (!ModelState.IsValid)
            return View(model);

        try
        {
            if (model.TipoUsuario == TipoUsuario.Bibliotecario)
            {
                await _usuarioService.CriarBibliotecarioAsync(
                    operador,
                    model.Nome,
                    model.CPF,
                    model.Email,
                    model.Senha,
                    model.Matricula ?? string.Empty);

                TempData["Sucesso"] = "Bibliotecário cadastrado com sucesso!";
            }
            else
            {
                if (!model.Perfil.HasValue)
                    throw new RegraNegocioException("Selecione o perfil do cliente.");

                await _usuarioService.CriarClienteAsync(
                    operador,
                    model.Nome,
                    model.CPF,
                    model.Email,
                    model.Senha,
                    model.Perfil.Value,
                    model.RegistroInstitucional ?? string.Empty);

                TempData["Sucesso"] = "Cliente cadastrado com sucesso!";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var operador = await ObterOperadorLogadoAsync();
            var alvo = await _usuarioService.ObterPorIdAsync(id);

            if (alvo is Administrador)
            {
                TempData["Erro"] = "Contas de administrador não podem ser editadas por esta tela.";
                return RedirectToAction(nameof(Index));
            }

            if (alvo is Bibliotecario && !operador.PossuiPermissao(TipoOperacao.GerenciarBibliotecarios))
                return RedirectToAction("AcessoNegado", "Account");

            var model = new UsuarioEditarViewModel
            {
                ID = alvo.ID,
                Nome = alvo.Nome,
                CPF = alvo.CPF,
                Email = alvo.Email,
                TipoUsuario = alvo is Bibliotecario ? TipoUsuario.Bibliotecario : TipoUsuario.Cliente,
                Perfil = alvo is Cliente c ? c.Perfil : null,
                RegistroInstitucional = alvo is Cliente cli ? cli.RegistroInstitucional : null,
                Matricula = alvo is Bibliotecario b ? b.Matricula : null
            };

            return View(model);
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UsuarioEditarViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var operador = await ObterOperadorLogadoAsync();

            await _usuarioService.AtualizarAsync(
                operador,
                model.ID,
                model.Nome,
                model.Email,
                model.NovaSenha,
                model.Perfil,
                model.RegistroInstitucional,
                model.Matricula);

            TempData["Sucesso"] = "Usuário atualizado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AlternarStatus(int id)
    {
        try
        {
            var operador = await ObterOperadorLogadoAsync();
            await _usuarioService.AlternarStatusAsync(operador, id);
            TempData["Sucesso"] = "Status do usuário atualizado com sucesso!";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var operador = await ObterOperadorLogadoAsync();
            await _usuarioService.ExcluirAsync(operador, id);
            TempData["Sucesso"] = "Usuário excluído com sucesso!";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
