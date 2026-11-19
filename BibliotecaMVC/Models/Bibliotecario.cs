using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;

namespace BibliotecaMVC.Models;

public class Bibliotecario : Usuario
{
    public string Matricula {get; private set;}

    // construtor para criar novos bibliotecários
    public Bibliotecario(string nome, string cpf, string email, string senhaHash, string matricula) : base(nome, cpf, email, senhaHash)
    {
        Matricula = ValidarMatricula(matricula);
    }
    
    // construtor para reconstituição do objeto pelo banco
    protected Bibliotecario(int id, string nome, string cpf, string email, string senhaHash, DateTime dataCadastro, bool cadastroAtivo, string matricula) : base (id, nome, cpf, email, senhaHash, dataCadastro, cadastroAtivo)
    {
        Matricula = matricula;
    }

    // método que chama o construtor de reconstituição, permitindo que UsuarioRepository possa chamá-lo
    internal static Bibliotecario Reconstituir(int id, string nome, string cpf, string email, string senhaHash, DateTime dataCadastro, bool cadastroAtivo, string matricula)
    {
        return new Bibliotecario(id, nome, cpf, email, senhaHash, dataCadastro, cadastroAtivo, matricula);
    }

    public void AtualizarMatricula(string matricula)
    {
        Matricula = ValidarMatricula(matricula);
    }
    
    //Implementação das regras de permissão para bibliotecario
    public override bool PossuiPermissao(TipoOperacao operacao)
    {
        if (!CadastroAtivo)
            return false;

        if (!Enum.IsDefined(operacao))
            return false;

        return operacao != TipoOperacao.GerenciarBibliotecarios; // bibliotecário não pode gerenciar outros bibliotecários
    }
    

    private static string ValidarMatricula(string matricula)
    {
        if (string.IsNullOrWhiteSpace(matricula))
            throw new RegraNegocioException("A matrícula é obrigatória");

        return matricula.Trim();
    }   
}

