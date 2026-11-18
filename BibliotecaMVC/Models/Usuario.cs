using System.Net.Mail;
using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;

namespace BibliotecaMVC.Models;

public abstract class Usuario
{
    public int ID { get; private set; }
    public string Nome { get; private set; }
    public string CPF { get; private set; }
    public string Email { get; private set; }
    public string SenhaHash { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public bool CadastroAtivo { get; private set; }

    // construtor para criar novos usuários
    protected Usuario(string nome, string cpf, string email, string senhaHash)
    {
        Nome = ValidarNome(nome);
        CPF = ValidarCPF(cpf);
        Email = ValidarEmail(email);
        SenhaHash = ValidarSenha(senhaHash);
        DataCadastro = DateTime.Now;
        CadastroAtivo = true;
    }

    // construtor para RECONSTITUIÇÃO do objeto pelo banco
    protected Usuario(int id, string nome, string cpf, string email, string senhaHash, DateTime dataCadastro, bool cadastroAtivo)
    {
        ID = id;
        Nome = nome;
        CPF = cpf;
        Email = email;
        SenhaHash = senhaHash;
        DataCadastro = dataCadastro;
        CadastroAtivo = cadastroAtivo;
    }

    public void AtualizarDados(string nome, string email)
    {
        Nome = ValidarNome(nome);
        Email = ValidarEmail(email);
    }
    public void AlterarSenha(string novaSenhaHash)
    {
        SenhaHash = ValidarSenha(novaSenhaHash);
    }

    public void Ativar()
    {
        if (CadastroAtivo)
            throw new RegraNegocioException("O cadastro já está ativo");

        CadastroAtivo = true;
    }
    public void Desativar()
    {
        if (!CadastroAtivo)
            throw new RegraNegocioException("O cadastro já está desativado");

        CadastroAtivo = false;
    }

    public abstract bool PossuiPermissao(TipoOperacao operacao);

    private static string ValidarNome(string nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new RegraNegocioException("O nome de usuário é obrigatório.");

        return nome.Trim();
    }
    private static string ValidarCPF(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new RegraNegocioException("O CPF é obrigatório.");

        var cpfLimpo = cpf.Replace(".", "").Replace("-", "").Trim();

        if (cpfLimpo.Length != 11 || !cpfLimpo.All(char.IsDigit))
            throw new RegraNegocioException("O CPF deve conter 11 digitos numéricos");

        return cpfLimpo;
    }
   private static string ValidarEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new RegraNegocioException("O e-mail é obrigatório");

        var emailLimpo = email.Trim();

        if (!MailAddress.TryCreate(emailLimpo, out _)) // "out _" descarta o objeto MailAddress criado, retornando apenas o booleano da operação
            throw new RegraNegocioException("O e-mail informado é inválido");

        return emailLimpo;
    }
    private static string ValidarSenha(string senha)
    {
        if (string.IsNullOrWhiteSpace(senha))
            throw new RegraNegocioException("A senha é obrigatória");

        return senha;
    }
}