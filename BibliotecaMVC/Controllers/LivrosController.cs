using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Filters;
using BibliotecaMVC.Services;
using BibliotecaMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaMVC.Controllers;

public class LivrosController : Controller
{
    private readonly LivroService _livroService;
    private readonly CategoriaService _categoriaService;

    public LivrosController(LivroService livroService, CategoriaService categoriaService)
    {
        _livroService = livroService;
        _categoriaService = categoriaService;
    }

    private async Task<IEnumerable<SelectListItem>> CarregarCategoriasSelectAsync(int? selecionada = null)
    {
        var categorias = await _categoriaService.ListarTodasAsync();
        return categorias.Select(c => new SelectListItem
        {
            Value = c.ID.ToString(),
            Text = c.Nome,
            Selected = selecionada.HasValue && c.ID == selecionada.Value
        });
    }

    [HttpGet]
    [AutorizarOperacao(TipoOperacao.ConsultarCatalogo)]
    public async Task<IActionResult> Index(string? termo, int? categoriaId, string? disponibilidade, string? ordenacao)
    {
        var model = await _livroService.ConsultarCatalogoAsync(termo, categoriaId, disponibilidade, ordenacao);
        return View(model);
    }

    [HttpGet]
    [AutorizarOperacao(TipoOperacao.ConsultarCatalogo)]
    public async Task<IActionResult> Details(int id)
    {
        try
        {
            var model = await _livroService.ObterDetalhesAsync(id);
            return View(model);
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    [HttpGet]
    [AutorizarOperacao(TipoOperacao.GerenciarLivros)]
    public async Task<IActionResult> Create()
    {
        var categorias = (await CarregarCategoriasSelectAsync()).ToList();

        if (categorias.Count == 0)
        {
            TempData["Erro"] = "Cadastre ao menos uma categoria antes de cadastrar um livro (RN07).";
            return RedirectToAction("Create", "Categorias");
        }

        var model = new LivroFormViewModel { Categorias = categorias };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AutorizarOperacao(TipoOperacao.GerenciarLivros)]
    public async Task<IActionResult> Create(LivroFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categorias = await CarregarCategoriasSelectAsync(model.CategoriaID);
            return View(model);
        }

        try
        {
            var novoId = await _livroService.CriarAsync(
                model.Titulo,
                model.ISBN,
                model.Autor,
                model.Resumo,
                model.CategoriaID);

            TempData["Sucesso"] = "Livro cadastrado com sucesso!";
            return RedirectToAction(nameof(Details), new { id = novoId });
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Categorias = await CarregarCategoriasSelectAsync(model.CategoriaID);
            return View(model);
        }
    }

    [HttpGet]
    [AutorizarOperacao(TipoOperacao.GerenciarLivros)]
    public async Task<IActionResult> Edit(int id)
    {
        try
        {
            var livro = await _livroService.ObterPorIdAsync(id);
            var model = new LivroFormViewModel
            {
                ID = livro.ID,
                Titulo = livro.Titulo,
                Autor = livro.Autor,
                ISBN = livro.ISBN,
                Resumo = livro.Resumo,
                CategoriaID = livro.Categoria.ID,
                Categorias = await CarregarCategoriasSelectAsync(livro.Categoria.ID)
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
    [AutorizarOperacao(TipoOperacao.GerenciarLivros)]
    public async Task<IActionResult> Edit(LivroFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categorias = await CarregarCategoriasSelectAsync(model.CategoriaID);
            return View(model);
        }

        try
        {
            await _livroService.AtualizarAsync(
                model.ID,
                model.Titulo,
                model.ISBN,
                model.Autor,
                model.Resumo,
                model.CategoriaID);

            TempData["Sucesso"] = "Livro atualizado com sucesso!";
            return RedirectToAction(nameof(Details), new { id = model.ID });
        }
        catch (RegraNegocioException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Categorias = await CarregarCategoriasSelectAsync(model.CategoriaID);
            return View(model);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [AutorizarOperacao(TipoOperacao.GerenciarLivros)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _livroService.ExcluirAsync(id);
            TempData["Sucesso"] = "Livro excluído com sucesso!";
        }
        catch (RegraNegocioException ex)
        {
            TempData["Erro"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
