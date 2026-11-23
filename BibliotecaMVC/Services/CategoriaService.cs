using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;

namespace BibliotecaMVC.Services;

public class CategoriaService
{
    private readonly ICategoriaRepository _categoriaRepository;

    public CategoriaService(ICategoriaRepository categoriaRepository)
    {
        _categoriaRepository = categoriaRepository;
    }

    public async Task<IEnumerable<Categoria>> ListarTodasAsync()
    {
        return await _categoriaRepository.ObterTodosAsync();
    }

    public async Task<Categoria> ObterPorIdAsync(int id)
    {
        return await _categoriaRepository.ObterPorIdAsync(id)
            ?? throw new RegraNegocioException("Categoria não encontrada.");
    }

    public async Task<int> CriarAsync(string nome, string descricao)
    {
        var categoria = new Categoria(nome, descricao);

        var existente = await _categoriaRepository.ObterPorNomeAsync(categoria.Nome);
        if (existente is not null)
            throw new RegraNegocioException("Já existe uma categoria cadastrada com este nome.");

        return await _categoriaRepository.AdicionarAsync(categoria);
    }

    public async Task AtualizarAsync(int id, string nome, string descricao)
    {
        var categoria = await ObterPorIdAsync(id);

        categoria.AlterarNome(nome);
        categoria.AlterarDescricao(descricao);

        var existente = await _categoriaRepository.ObterPorNomeAsync(categoria.Nome);
        if (existente is not null && existente.ID != categoria.ID)
            throw new RegraNegocioException("Já existe outra categoria cadastrada com este nome.");

        await _categoriaRepository.AtualizarAsync(categoria);
    }

    public async Task ExcluirAsync(int id)
    {
        _ = await ObterPorIdAsync(id);

        if (await _categoriaRepository.PossuiLivrosAsync(id))
            throw new RegraNegocioException(
                "Esta categoria possui livros associados e não pode ser excluída (RN08).");

        await _categoriaRepository.RemoverAsync(id);
    }
}
