using BibliotecaMVC.Enums;
using BibliotecaMVC.Exceptions;

namespace BibliotecaMVC.Models;

public class Bibliotecario : Usuario
{
    public string Matricula {get; private set;}

    public Bibliotecario(string nome, string cpf, string email, string senhaHash, string matricula) : base(nome, cpf, email, senhaHash)
    {
        Matricula = ValidarMatricula(matricula);
    }
    
    // construtor para reconstituição do objeto pelo banco
    protected Bibliotecario(string nome, string cpf, string email, string senhaHash, string matricula) : base (nome, cpf, email, senhaHash)
    {
        Matricula = matricula;
    }

    // construtor para criar novos bibliotecários
    public void ValidarMatricula(string matricula)
    {
        if (string.IsNullOrWhiteSpace(matricula))
            throw new RegraNegocioException("A matrícula é obrigatória");

        return matricula.Trim();
    }


}