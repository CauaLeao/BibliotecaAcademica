using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Filters;
using BibliotecaMVC.Services;
using BibliotecaMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BibliotecaMVC.Controllers;

[AutorizarOperacao(TipoOperacao.GerenciarCategorias)]
public class CategoriasController : Controller
{
    private readonly CategoriaService _categoriaService;

    public CategoriasController(CategoriaService categoriaService)
    {
        _categoriaService = categoriaService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var categorias = await _categoriaService.ListarTodasAsync();
        return View(categorias);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CategoriaViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoriaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _categoriaService.CriarAsync(model.Nome, model.Descricao);
            TempData["Sucesso"] = "Categoria cadastrada com sucesso!";
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
            var categoria = await _categoriaService.ObterPorIdAsync(id);
            var model = new CategoriaViewModel
            {
                ID = categoria.ID,
                Nome = categoria.Nome,
                Descricao = categoria.Descricao
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
    public async Task<IActionResult> Edit(CategoriaViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            await _categoriaService.AtualizarAsync(model.ID, model.Nome, model.Descricao);
            TempData["Sucesso"] = "Categoria atualizada com sucesso!";
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
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _categoriaService.ExcluirAsync(id);
            TempData["Sucesso"] = "Categoria excluída com sucesso!";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
