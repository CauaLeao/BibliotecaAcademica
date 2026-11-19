using BibliotecaMVC.Enums;

namespace BibliotecaMVC.Models;

public class Administrador : Bibliotecario 
{
	//Construtor publico para criar um novo administrador
	public Administrador(string nome, string cpf, string email, string senhaHash, string matricula)
		:base(nome, cpf, email, senhaHash, matricula)
	{
	}

	//Construtor protegido para a reconstituição do administrdor pelo banco
	protected Administrador(int id, string nome, string cpf, string email, string senhaHash, DateTime dataCadastro, bool cadastroAtivo, string matricula)
		:base(id, nome, cpf, email, senhaHash, dataCadastro, cadastroAtivo, matricula)
	{
	}

    //Ponto de hidratação para o Dapper
    internal static new Administrador Reconstituir(int id, string nome, string cpf, string email, string senhaHash, DateTime dataCadastro, bool cadastroAtivo, string matricula)
    {
        return new Administrador(id, nome, cpf, email, senhaHash, dataCadastro, cadastroAtivo, matricula);
    }

    public override bool PossuiPermissao(TipoOperacao operacao)
    {
        // Se inativo ou sem enum, não permite. Protegendo contra erros de digitação ou enum inexistente.
        if (!CadastroAtivo || !Enum.IsDefined(operacao))
            return false;

		return true; //Um usuario administrador possui todas as permissões, desde que esteja ativo.
    }
}
