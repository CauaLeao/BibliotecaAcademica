namespace BibliotecaMVC.Enums;

public enum TipoOperacao // lista de permissões a serem concedidas aos usuários
{
    ConsultarCatalogo = 1,
    ConsultarDisponibilidade = 2,
    ConsultarPropriosEmprestimos = 3,
    ConsultarTodosEmprestimos = 4,
    GerenciarClientes = 5,
    GerenciarLivros = 6,
    GerenciarCategorias = 7,
    GerenciarExemplares = 8,
    RegistrarEmprestimo = 9,
    RegistrarDevolucao = 10,
    RenovarEmprestimo = 11,
    GerenciarBibliotecarios = 12
}