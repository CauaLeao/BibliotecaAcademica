using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;
using BibliotecaMVC.Models;
using BibliotecaMVC.Repositories.Interfaces;

namespace BibliotecaMVC.Services;

public class UsuarioService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public UsuarioService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<IEnumerable<Usuario>> ListarParaOperadorAsync(Usuario operador)
    {
        var todos = await _usuarioRepository.ObterTodosAsync();

        // Administrador enxerga todos; Bibliotecário enxerga apenas Clientes
        if (operador.PossuiPermissao(TipoOperacao.GerenciarBibliotecarios))
            return todos;

        return todos.Where(u => u is Cliente);
    }

    public async Task<Usuario> ObterPorIdAsync(int id)
    {
        return await _usuarioRepository.ObterPorIdAsync(id)
            ?? throw new RegraNegocioException("Usuário não encontrado.");
    }

    public async Task<int> CriarClienteAsync(
        Usuario operador,
        string nome,
        string cpf,
        string email,
        string senha,
        TipoCliente perfil,
        string registroInstitucional)
    {
        if (!operador.PossuiPermissao(TipoOperacao.GerenciarClientes))
            throw new RegraNegocioException("Você não tem permissão para cadastrar clientes.");

        if (string.IsNullOrWhiteSpace(senha))
            throw new RegraNegocioException("A senha é obrigatória.");

        var senhaHash = AuthService.GerarHashSenha(senha);
        var cliente = new Cliente(nome, cpf, email, senhaHash, perfil, registroInstitucional);

        await ValidarUnicidadeBaseAsync(cliente.CPF, cliente.Email, ignorarId: null);

        var existenteRi = await _usuarioRepository.ObterPorRegistroInstitucionalAsync(cliente.RegistroInstitucional);
        if (existenteRi is not null)
            throw new RegraNegocioException("Já existe um usuário cadastrado com este RA / Registro Institucional (RN02).");

        return await _usuarioRepository.AdicionarAsync(cliente);
    }

    public async Task<int> CriarBibliotecarioAsync(
        Usuario operador,
        string nome,
        string cpf,
        string email,
        string senha,
        string matricula)
    {
        if (!operador.PossuiPermissao(TipoOperacao.GerenciarBibliotecarios))
            throw new RegraNegocioException("Apenas administradores podem cadastrar bibliotecários (RN06).");

        if (string.IsNullOrWhiteSpace(senha))
            throw new RegraNegocioException("A senha é obrigatória.");

        var senhaHash = AuthService.GerarHashSenha(senha);
        var bibliotecario = new Bibliotecario(nome, cpf, email, senhaHash, matricula);

        await ValidarUnicidadeBaseAsync(bibliotecario.CPF, bibliotecario.Email, ignorarId: null);

        var existenteMat = await _usuarioRepository.ObterPorMatriculaAsync(bibliotecario.Matricula);
        if (existenteMat is not null)
            throw new RegraNegocioException("Já existe um usuário cadastrado com esta matrícula (RN02).");

        return await _usuarioRepository.AdicionarAsync(bibliotecario);
    }

    public async Task AtualizarAsync(
        Usuario operador,
        int id,
        string nome,
        string email,
        string? novaSenha,
        TipoCliente? perfil,
        string? registroInstitucional,
        string? matricula)
    {
        var alvo = await ObterPorIdAsync(id);
        ValidarPermissaoSobreAlvo(operador, alvo);

        alvo.AtualizarDados(nome, email);
        await ValidarUnicidadeEmailAsync(alvo.Email, ignorarId: alvo.ID);

        if (alvo is Cliente cliente)
        {
            if (!perfil.HasValue || string.IsNullOrWhiteSpace(registroInstitucional))
                throw new RegraNegocioException("Perfil e registro institucional são obrigatórios para clientes.");

            cliente.AtualizarVinculo(perfil.Value, registroInstitucional);

            var existenteRi = await _usuarioRepository.ObterPorRegistroInstitucionalAsync(cliente.RegistroInstitucional);
            if (existenteRi is not null && existenteRi.ID != cliente.ID)
                throw new RegraNegocioException("Já existe outro usuário com este RA / Registro Institucional (RN02).");
        }
        else if (alvo is Bibliotecario bibliotecario)
        {
            if (string.IsNullOrWhiteSpace(matricula))
                throw new RegraNegocioException("A matrícula é obrigatória para bibliotecários.");

            bibliotecario.AtualizarMatricula(matricula);

            var existenteMat = await _usuarioRepository.ObterPorMatriculaAsync(bibliotecario.Matricula);
            if (existenteMat is not null && existenteMat.ID != bibliotecario.ID)
                throw new RegraNegocioException("Já existe outro usuário com esta matrícula (RN02).");
        }

        if (!string.IsNullOrWhiteSpace(novaSenha))
            alvo.AlterarSenha(AuthService.GerarHashSenha(novaSenha));

        await _usuarioRepository.AtualizarAsync(alvo);
    }

    public async Task AlternarStatusAsync(Usuario operador, int id)
    {
        var alvo = await ObterPorIdAsync(id);
        ValidarPermissaoSobreAlvo(operador, alvo);

        if (alvo.CadastroAtivo)
            alvo.Desativar();
        else
            alvo.Ativar();

        await _usuarioRepository.AtualizarAsync(alvo);
    }

    public async Task ExcluirAsync(Usuario operador, int id)
    {
        var alvo = await ObterPorIdAsync(id);
        ValidarPermissaoSobreAlvo(operador, alvo);

        if (await _usuarioRepository.PossuiEmprestimosAsync(id))
            throw new RegraNegocioException(
                "Usuários com histórico de empréstimos não podem ser excluídos. Utilize a inativação cadastral (RN23).");

        await _usuarioRepository.RemoverAsync(id);
    }

    private static void ValidarPermissaoSobreAlvo(Usuario operador, Usuario alvo)
    {
        if (alvo is Administrador)
            throw new RegraNegocioException("Contas de administrador não podem ser alteradas ou excluídas pelo sistema.");

        if (alvo is Bibliotecario && !operador.PossuiPermissao(TipoOperacao.GerenciarBibliotecarios))
            throw new RegraNegocioException("Apenas administradores podem gerenciar contas de bibliotecários (RN06).");

        if (alvo is Cliente && !operador.PossuiPermissao(TipoOperacao.GerenciarClientes))
            throw new RegraNegocioException("Você não tem permissão para gerenciar contas de clientes.");
    }

    private async Task ValidarUnicidadeBaseAsync(string cpf, string email, int? ignorarId)
    {
        var existenteCpf = await _usuarioRepository.ObterPorCpfAsync(cpf);
        if (existenteCpf is not null && existenteCpf.ID != ignorarId)
            throw new RegraNegocioException("Já existe um usuário cadastrado com este CPF (RN01).");

        await ValidarUnicidadeEmailAsync(email, ignorarId);
    }

    private async Task ValidarUnicidadeEmailAsync(string email, int? ignorarId)
    {
        var existenteEmail = await _usuarioRepository.ObterPorEmailAsync(email);
        if (existenteEmail is not null && existenteEmail.ID != ignorarId)
            throw new RegraNegocioException("Já existe um usuário cadastrado com este e-mail (RN01).");
    }
}