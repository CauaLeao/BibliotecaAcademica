using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;

namespace BibliotecaMVC.Services;

public class ExemplarService
{
    private readonly IExemplarRepository _exemplarRepository;
    private readonly ILivroRepository _livroRepository;

    public ExemplarService(IExemplarRepository exemplarRepository, ILivroRepository livroRepository)
    {
        _exemplarRepository = exemplarRepository;
        _livroRepository = livroRepository;
    }

    public async Task<Exemplar> ObterPorIdAsync(int id)
    {
        return await _exemplarRepository.ObterPorIdAsync(id)
            ?? throw new RegraNegocioException("Exemplar não encontrado.");
    }

    public async Task<int> CriarAsync(
        int livroId,
        string codigo,
        EstadoConservacao estadoConservacao,
        DateTime dataAquisicao)
    {
        var livro = await _livroRepository.ObterPorIdAsync(livroId)
            ?? throw new RegraNegocioException("Livro não encontrado (RN09).");

        if (dataAquisicao.Date > DateTime.Today)
            throw new RegraNegocioException("A data de aquisição não pode ser futura.");

        // Composição: o próprio Livro cria e valida o Exemplar
        var exemplar = livro.CriarExemplar(codigo, estadoConservacao, dataAquisicao);

        var existente = await _exemplarRepository.ObterPorCodigoAsync(exemplar.Codigo);
        if (existente is not null)
            throw new RegraNegocioException("Já existe um exemplar cadastrado com este código (RN09).");

        return await _exemplarRepository.AdicionarAsync(exemplar);
    }

    public async Task AtualizarAsync(
        int id,
        string codigo,
        EstadoConservacao estadoConservacao,
        DateTime dataAquisicao)
    {
        var exemplar = await ObterPorIdAsync(id);

        if (dataAquisicao.Date > DateTime.Today)
            throw new RegraNegocioException("A data de aquisição não pode ser futura.");

        exemplar.AtualizarDados(codigo, dataAquisicao);

        var existente = await _exemplarRepository.ObterPorCodigoAsync(exemplar.Codigo);
        if (existente is not null && existente.ID != exemplar.ID)
            throw new RegraNegocioException("Já existe outro exemplar cadastrado com este código (RN09).");

        if (exemplar.EstadoConservacao != estadoConservacao)
            exemplar.AlterarConservacao(estadoConservacao);

        await _exemplarRepository.AtualizarAsync(exemplar);
    }

    public async Task TornarIndisponivelAsync(int id)
    {
        var exemplar = await ObterPorIdAsync(id);
        exemplar.TornarIndisponivel();
        await _exemplarRepository.AtualizarAsync(exemplar);
    }

    public async Task DisponibilizarAsync(int id)
    {
        var exemplar = await ObterPorIdAsync(id);
        exemplar.Disponibilizar();
        await _exemplarRepository.AtualizarAsync(exemplar);
    }

    public async Task ExcluirAsync(int id)
    {
        var exemplar = await ObterPorIdAsync(id);

        if (await _exemplarRepository.PossuiEmprestimosAsync(id))
            throw new RegraNegocioException(
                "Este exemplar possui histórico de empréstimos e não pode ser excluído (RN10).");

        exemplar.Livro.RemoverExemplar(exemplar);

        await _exemplarRepository.RemoverAsync(id);
    }
}
