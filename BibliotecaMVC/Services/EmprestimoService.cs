using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;

namespace BibliotecaMVC.Services;

public class EmprestimoService
{
    private readonly IEmprestimoRepository _emprestimoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IExemplarRepository _exemplarRepository;

    public EmprestimoService(
        IEmprestimoRepository emprestimoRepository,
        IUsuarioRepository usuarioRepository,
        IExemplarRepository exemplarRepository)
    {
        _emprestimoRepository = emprestimoRepository;
        _usuarioRepository = usuarioRepository;
        _exemplarRepository = exemplarRepository;
    }

    public async Task<IEnumerable<Emprestimo>> ListarTodosAsync()
        => await _emprestimoRepository.ObterTodosAsync();

    public async Task<IEnumerable<Emprestimo>> ListarPorUsuarioAsync(int usuarioId)
        => await _emprestimoRepository.ObterPorUsuarioIdAsync(usuarioId);

    public async Task<IEnumerable<Emprestimo>> ListarAtivosAsync()
        => await _emprestimoRepository.ObterAtivosAsync();

    public async Task<Emprestimo> ObterPorIdAsync(int id)
    {
        var emp = await _emprestimoRepository.ObterPorIdAsync(id);
        return emp ?? throw new RegraNegocioException("Empréstimo não encontrado.");
    }

   
    public async Task<int> RegistrarEmprestimoAsync(int usuarioId, int exemplarId)
    {
        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId)
            ?? throw new RegraNegocioException("Usuário não encontrado.");

        if (!usuario.CadastroAtivo)
            throw new RegraNegocioException("Usuários inativos não podem realizar novos empréstimos");

        var exemplar = await _exemplarRepository.ObterPorIdAsync(exemplarId)
            ?? throw new RegraNegocioException("Exemplar não encontrado.");

        //  Verifica se o exemplar já possui empréstimo ativo
        var emprestimoAtivoExemplar = await _emprestimoRepository.ObterAtivoPorExemplarIdAsync(exemplarId);
        if (emprestimoAtivoExemplar != null)
            throw new RegraNegocioException("Este exemplar já se encontra emprestado para outro usuário.");

        //  Limite de empréstimos ativos simultâneos
        var emprestimosAtivosUsuario = await _emprestimoRepository.ObterAtivosPorUsuarioIdAsync(usuarioId);
        int limite = usuario is Cliente c ? c.ObterLimiteEmprestimos() : 3;

        if (emprestimosAtivosUsuario.Count() >= limite)
            throw new RegraNegocioException($"Limite máximo de {limite} empréstimos simultâneos atingido para este perfil.");

        //  Prazo de devolução
        int prazo = usuario is Cliente cliente ? cliente.ObterPrazoEmprestimo() : 10;

        // Cria a entidade no domínio (ela altera a disponibilidade do exemplar para Emprestado)
        var novoEmprestimo = new Emprestimo(usuario, exemplar, DateTime.Now, prazo);

        var idGerado = await _emprestimoRepository.AdicionarAsync(novoEmprestimo);
        await _exemplarRepository.AtualizarAsync(exemplar); // Persiste a nova disponibilidade do exemplar

        return idGerado;
    }

    //  Renovar empréstimo
    public async Task RenovarAsync(int emprestimoId)
    {
        var emp = await ObterPorIdAsync(emprestimoId);
        emp.Renovar(DateTime.Now); // Executa as regras de validação no domínio
        await _emprestimoRepository.AtualizarAsync(emp);
    }

    //  Registrar devolução
    public async Task RegistrarDevolucaoAsync(int emprestimoId, EstadoConservacao estadoDevolucao, bool impedimento = false)
    {
        var emp = await ObterPorIdAsync(emprestimoId);
        emp.RegistrarDevolucao(DateTime.Now);

        // Atualiza a disponibilidade do exemplar no domínio (RN19)
        emp.Exemplar.RegistrarDevolucao(estadoDevolucao, impedimento);

        await _emprestimoRepository.AtualizarAsync(emp);
        await _exemplarRepository.AtualizarAsync(emp.Exemplar);
    }
}