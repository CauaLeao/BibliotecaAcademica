using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Filters;
using BibliotecaMVC.Models;
using BibliotecaMVC.Services;
using BibliotecaMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaMVC.Controllers;

public class EmprestimosController : Controller
{
    private readonly EmprestimoService _emprestimoService;

    public EmprestimosController(EmprestimoService emprestimoService)
    {
        _emprestimoService = emprestimoService;
    }

    private async Task CarregarListasFormularioAsync(EmprestimoFormViewModel model)
    {
        var usuarios = await _emprestimoService.ListarUsuariosAtivosAsync();
        var exemplares = await _emprestimoService.ListarExemplaresDisponiveisAsync();

        model.Usuarios = usuarios.Select(u =>
        {
            var descricaoPerfil = u switch
            {
                Cliente c => $"{c.Perfil} - {c.ObterPrazoEmprestimo()} dias",
                Administrador => "Administrador - 10 dias",
                Bibliotecario => "Bibliotecário - 10 dias",
                _ => "Usuário"
            };

            return new SelectListItem
            {
                Value = u.ID.ToString(),
                Text = $"{u.Nome} ({descricaoPerfil})",
                Selected = model.UsuarioID == u.ID
            };
        });

        model.Exemplares = exemplares.Select(e => new SelectListItem
        {
            Value = e.ID.ToString(),
            Text = $"{e.Codigo} — {e.Livro.Titulo} ({e.EstadoConservacao})",
            Selected = model.ExemplarID == e.ID
        });
    }

    // Visão do Leitor: Seus próprios empréstimos
    [HttpGet]
    [AutorizarOperacao(TipoOperacao.ConsultarPropriosEmprestimos)]
    public async Task<IActionResult> Meus()
    {
        var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;
        var emprestimos = await _emprestimoService.ListarPorUsuarioAsync(usuarioId);
        return View("Usuario", emprestimos);
    }

    // Visão do Gestor: Todos os empréstimos com filtros por situação (RF16) e prazo (RF17)
    [HttpGet]
    [AutorizarOperacao(TipoOperacao.ConsultarTodosEmprestimos)]
    public async Task<IActionResult> Index(string? situacao, string? prazo)
    {
        var model = await _emprestimoService.ListarFiltradosAsync(situacao, prazo);
        return View(model);
    }

    // Registro de novo empréstimo (RF13 - Apenas Bibliotecário / Admin)
    [HttpGet]
    [AutorizarOperacao(TipoOperacao.RegistrarEmprestimo)]
    public async Task<IActionResult> Create(int? exemplarId = null)
    {
        var model = new EmprestimoFormViewModel
        {
            ExemplarID = exemplarId ?? 0
        };

        await CarregarListasFormularioAsync(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AutorizarOperacao(TipoOperacao.RegistrarEmprestimo)]
    public async Task<IActionResult> Create(EmprestimoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            await CarregarListasFormularioAsync(model);
            return View(model);
        }

        try
        {
            await _emprestimoService.RegistrarEmprestimoAsync(model.UsuarioID, model.ExemplarID);
            TempData["Sucesso"] = "Empréstimo registrado com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            await CarregarListasFormularioAsync(model);
            return View(model);
        }
    }

    // Renovar empréstimo
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AutorizarOperacao(TipoOperacao.ConsultarPropriosEmprestimos)]
    public async Task<IActionResult> Renovar(int id)
    {
        var usuarioId = HttpContext.Session.GetInt32("UsuarioId")!.Value;
        var tipo = HttpContext.Session.GetString("TipoUsuario");
        var isGestor = tipo == "Bibliotecario" || tipo == "Administrador";

        try
        {
            if (!isGestor)
            {
                var emprestimo = await _emprestimoService.ObterPorIdAsync(id);
                if (emprestimo.Usuario.ID != usuarioId)
                    return RedirectToAction("AcessoNegado", "Account");
            }

            await _emprestimoService.RenovarAsync(id);
            TempData["Sucesso"] = "Empréstimo renovado com sucesso!";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return isGestor
            ? RedirectToAction(nameof(Index))
            : RedirectToAction(nameof(Meus));
    }

    // Devolução de exemplar (Apenas Bibliotecário / Admin)
    [HttpPost]
    [ValidateAntiForgeryToken]
    [AutorizarOperacao(TipoOperacao.RegistrarDevolucao)]
    public async Task<IActionResult> Devolver(int id, EstadoConservacao estadoConservacao)
    {
        try
        {
            await _emprestimoService.RegistrarDevolucaoAsync(id, estadoConservacao);
            TempData["Sucesso"] = "Devolução registrada e exemplar atualizado com sucesso!";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
