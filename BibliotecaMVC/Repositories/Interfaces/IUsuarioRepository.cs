using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories.Interfaces;

// opera a tabela de usuários do banco
public interface IUsuarioRepository : IRepository<Usuario> // nota: Task<Usuario?> deve retornar um usuário que pode ser nulo
{
    Task<Usuario?> ObterPorEmailAsync(string email); // busca usuário por e-mail
    Task<Usuario?> ObterPorCpfAsync(string cpf); // busca usuário por CPF
    Task<Usuario?> ObterPorMatriculaAsync(string matricula); // busca usuário por matrícula
    Task<Usuario?> ObterPorRegistroInstitucionalAsync(string registroInstitucional); // busca usuário por RI
    Task<bool> PossuiEmprestimosAsync(int usuarioId); // verifica, de acordo com o ID, se o usuário possui emrpéstimos
}