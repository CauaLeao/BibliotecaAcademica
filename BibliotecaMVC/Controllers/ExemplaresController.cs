using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Filters;
using BibliotecaMVC.Services;
using BibliotecaMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

[AutorizarOperacao(TipoOperacao.GerenciarExemplares)]
public class ExemplaresController : Controller
{
    private readonly ExemplarService _exemplarService;
    private readonly LivroService _livroService;

    public ExemplaresController(ExemplarService exemplarService, LivroService livroService)
    {
        _exemplarService = exemplarService;
        _livroService = livroService;
    }

    [HttpGet]
    public async Task<IActionResult> Create(int livroId)
    {
        try
        {
            var livro = await _livroService.ObterPorIdAsync(livroId);
            var model = new ExemplarFormViewModel
            {
                LivroID = livro.ID,
                LivroTitulo = livro.Titulo,
                DataAquisicao = DateTime.Today
            };
            return View(model);
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction("Index", "Livros");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ExemplarFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _exemplarService.CriarAsync(
                model.LivroID,
                model.Codigo,
                model.EstadoConservacao,
                model.DataAquisicao);

            TempData["Sucesso"] = "Exemplar cadastrado com sucesso!";
            return RedirectToAction("Details", "Livros", new { id = model.LivroID });
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
            var exemplar = await _exemplarService.ObterPorIdAsync(id);
            var model = new ExemplarFormViewModel
            {
                ID = exemplar.ID,
                LivroID = exemplar.Livro.ID,
                LivroTitulo = exemplar.Livro.Titulo,
                Codigo = exemplar.Codigo,
                EstadoConservacao = exemplar.EstadoConservacao,
                DataAquisicao = exemplar.DataAquisicao,
                Disponibilidade = exemplar.Disponibilidade
            };
            return View(model);
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction("Index", "Livros");
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ExemplarFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _exemplarService.AtualizarAsync(
                model.ID,
                model.Codigo,
                model.EstadoConservacao,
                model.DataAquisicao);

            TempData["Sucesso"] = "Exemplar atualizado com sucesso!";
            return RedirectToAction("Details", "Livros", new { id = model.LivroID });
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> TornarIndisponivel(int id, int livroId)
    {
        try
        {
            await _exemplarService.TornarIndisponivelAsync(id);
            TempData["Sucesso"] = "Exemplar marcado como indisponível.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction("Details", "Livros", new { id = livroId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Disponibilizar(int id, int livroId)
    {
        try
        {
            await _exemplarService.DisponibilizarAsync(id);
            TempData["Sucesso"] = "Exemplar liberado para circulação.";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction("Details", "Livros", new { id = livroId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int livroId)
    {
        try
        {
            await _exemplarService.ExcluirAsync(id);
            TempData["Sucesso"] = "Exemplar excluído com sucesso!";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction("Details", "Livros", new { id = livroId });
    }
}
