namespace BibliotecaMVC.Enums;

public enum TipoCliente // discriminador do tipo de cliente, para validar as regras de negócio dos empréstimos
{
    Aluno = 1,
    Funcionario = 2,
    Professor = 3,
}