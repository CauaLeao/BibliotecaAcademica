namespace BibliotecaMVC.Enums;

public enum TipoUsuario // discriminador do tipo de usuário, não utilizado pela classe, mas para o armazenamento pelo Dapper
{
    Cliente = 1,
    Bibliotecario = 2,
    Administrador = 3,
}