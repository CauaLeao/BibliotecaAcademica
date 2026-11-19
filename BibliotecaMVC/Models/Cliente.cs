using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;

namespace BibliotecaMVC.Models;

public class Cliente : Usuario
{
	public TipoCliente Perfil { get; private set; }
	public string RegistroInstitucional { get; private set; }


    //Construtor para criar novos clientes
    public Cliente(string nome, string cpf, string email, string senhaHash, TipoCliente perfil, string registroInstitucional)
         : base(nome, cpf, email, senhaHash)
    {
        Perfil = ValidarPerfil(perfil);
        RegistroInstitucional = ValidarRegistroInstitucional(registroInstitucional);
    }

    protected Cliente(int id, string nome, string cpf, string email, string senhaHash, DateTime dataCadastro, bool cadastroAtivo, TipoCliente perfil, string registroInstitucional)
        :base(id, nome, cpf, email, senhaHash, dataCadastro, cadastroAtivo)

    {
        Perfil = perfil;
        RegistroInstitucional = registroInstitucional;
    }

    internal static Cliente Reconstituir(int id, string nome, string cpf, string email, string senhaHash, DateTime dataCadastro, bool cadastroAtivo, TipoCliente perfil, string registroInstitucional)
    {
        return new Cliente(id, nome, cpf, email, senhaHash, dataCadastro, cadastroAtivo, perfil, registroInstitucional);
    }


    public void AtualizarVinculo(TipoCliente perfil, string registroInstitucional)
    {
        Perfil = ValidarPerfil(perfil);
        RegistroInstitucional = ValidarRegistroInstitucional(registroInstitucional);
    }

    public override bool PossuiPermissao(TipoOperacao operacao)
    {
        if (!CadastroAtivo || !Enum.IsDefined(operacao))
            return false;

        // Clientes só podem acessar ao acervo e aos seus próprios empréstimos.
        return operacao == TipoOperacao.ConsultarCatalogo ||
               operacao == TipoOperacao.ConsultarDisponibilidade ||
               operacao == TipoOperacao.ConsultarPropriosEmprestimos;
    }

    //Limite de empréstimos simultâneos.
    public int ObterLimiteEmprestimos()
    {
        return Perfil switch
        {
            TipoCliente.Professor => 5,
            TipoCliente.Aluno or TipoCliente.Funcionario => 3,
            _ => throw new RegraNegocioException("Perfil de cliente desconhecido.")
        };
    }

    // Prazo de devolução em dias corridos
    public int ObterPrazoEmprestimo()
    {
        return Perfil switch
        {
            TipoCliente.Aluno => 7,
            TipoCliente.Funcionario => 10,
            TipoCliente.Professor => 14,
            _ => throw new RegraNegocioException("Perfil de cliente desconhecido.")
        };
    }

    //Verifica se o cliente está ativo e se possui permissão para realizar empréstimos
    private static TipoCliente ValidarPerfil(TipoCliente perfil)
    {
        if (!Enum.IsDefined(perfil))
            throw new RegraNegocioException("Tipo de cliente inválido.");
        return perfil;
    }

    //Verifica se o registro institucional (RA ou matrícula) não é nulo ou vazio
    private static string ValidarRegistroInstitucional(string registro)
    {
        if (string.IsNullOrWhiteSpace(registro))
            throw new RegraNegocioException("O registro institucional (RA ou Matrícula) é obrigatório.");
        return registro.Trim();
    }

}
	