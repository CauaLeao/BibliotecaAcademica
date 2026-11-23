using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;
using BibliotecaMVC.ViewModels;

namespace BibliotecaMVC.Services;

public class LivroService
{
    private readonly ILivroRepository _livroRepository;
    private readonly ICategoriaRepository _categoriaRepository;
    private readonly IExemplarRepository _exemplarRepository;

    public LivroService(
        ILivroRepository livroRepository,
        ICategoriaRepository categoriaRepository,
        IExemplarRepository exemplarRepository)
    {
        _livroRepository = livroRepository;
        _categoriaRepository = categoriaRepository;
        _exemplarRepository = exemplarRepository;
    }

    public async Task<LivroCatalogoViewModel> ConsultarCatalogoAsync(
        string? termo,
        int? categoriaId,
        string? disponibilidade,
        string? ordenacao)
    {
        var livros = await _livroRepository.BuscarAsync(termo, categoriaId);
        var todosExemplares = await _exemplarRepository.ObterTodosAsync();
        var categorias = await _categoriaRepository.ObterTodosAsync();

        var exemplaresPorLivro = todosExemplares
            .GroupBy(e => e.Livro.ID)
            .ToDictionary(g => g.Key, g => g.ToList());

        var itens = livros.Select(livro =>
        {
            var exemplares = exemplaresPorLivro.GetValueOrDefault(livro.ID) ?? [];
            var disponiveis = exemplares.Count(e => e.Disponibilidade == DisponibilidadeExemplar.Disponivel);

            return new LivroCatalogoItemViewModel
            {
                Livro = livro,
                TotalExemplares = exemplares.Count,
                ExemplaresDisponiveis = disponiveis
            };
        });

        // Filtro por disponibilidade (RF08)
        itens = disponibilidade switch
        {
            "disponiveis" => itens.Where(i => i.ExemplaresDisponiveis > 0),
            "indisponiveis" => itens.Where(i => i.ExemplaresDisponiveis == 0),
            _ => itens
        };

        // Ordenação alfabética (RF08)
        itens = ordenacao switch
        {
            "titulo_desc" => itens.OrderByDescending(i => i.Livro.Titulo, StringComparer.CurrentCultureIgnoreCase),
            "autor_asc" => itens.OrderBy(i => i.Livro.Autor, StringComparer.CurrentCultureIgnoreCase)
                                .ThenBy(i => i.Livro.Titulo, StringComparer.CurrentCultureIgnoreCase),
            _ => itens.OrderBy(i => i.Livro.Titulo, StringComparer.CurrentCultureIgnoreCase)
        };

        return new LivroCatalogoViewModel
        {
            Termo = termo,
            CategoriaId = categoriaId,
            Disponibilidade = disponibilidade,
            Ordenacao = string.IsNullOrWhiteSpace(ordenacao) ? "titulo_asc" : ordenacao,
            Categorias = categorias,
            Itens = itens.ToList()
        };
    }

    public async Task<Livro> ObterPorIdAsync(int id)
    {
        return await _livroRepository.ObterPorIdAsync(id)
            ?? throw new RegraNegocioException("Livro não encontrado.");
    }

    public async Task<LivroDetalhesViewModel> ObterDetalhesAsync(int id)
    {
        var livro = await ObterPorIdAsync(id);
        var exemplares = await _exemplarRepository.ObterPorLivroIdAsync(id);

        return new LivroDetalhesViewModel
        {
            Livro = livro,
            Exemplares = exemplares
        };
    }

    public async Task<int> CriarAsync(string titulo, string isbn, string autor, string resumo, int categoriaId)
    {
        var categoria = await _categoriaRepository.ObterPorIdAsync(categoriaId)
            ?? throw new RegraNegocioException("A categoria selecionada não existe (RN07).");

        var livro = new Livro(titulo, isbn, autor, resumo, categoria);

        var existente = await _livroRepository.ObterPorIsbnAsync(livro.ISBN);
        if (existente is not null)
            throw new RegraNegocioException("Já existe um livro cadastrado com este ISBN.");

        return await _livroRepository.AdicionarAsync(livro);
    }

    public async Task AtualizarAsync(int id, string titulo, string isbn, string autor, string resumo, int categoriaId)
    {
        var livro = await ObterPorIdAsync(id);

        var novaCategoria = await _categoriaRepository.ObterPorIdAsync(categoriaId)
            ?? throw new RegraNegocioException("A categoria selecionada não existe (RN07).");

        livro.AtualizarDados(titulo, isbn, autor, resumo);

        var existente = await _livroRepository.ObterPorIsbnAsync(livro.ISBN);
        if (existente is not null && existente.ID != livro.ID)
            throw new RegraNegocioException("Já existe outro livro cadastrado com este ISBN.");

        // Mantém ambos os lados da agregação coerentes
        livro.AlterarCategoria(novaCategoria);

        await _livroRepository.AtualizarAsync(livro);
    }

    public async Task ExcluirAsync(int id)
    {
        _ = await ObterPorIdAsync(id);

        if (await _livroRepository.PossuiExemplaresAsync(id))
            throw new RegraNegocioException(
                "Este livro possui exemplares associados e não pode ser excluído (RN10).");

        await _livroRepository.RemoverAsync(id);
    }
}
