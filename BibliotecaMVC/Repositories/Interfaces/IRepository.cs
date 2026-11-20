namespace BibliotecaMVC.Repositories.Interfaces;

// molde para os demais Repositories, que irão efetivamente realizar as chamadas no banco
public interface IRepository<T> where T: class // nota: métodos com Task<...> são assíncronos
{
    Task<T?> ObterPorIdAsync(int id); // busca 1 registro pelo ID
    Task<IEnumerable<T>> ObterTodosAsync(); // busca todos os registros da tabela
    Task<int> AdicionarAsync(T entidade); // salva um cadastro e retorna o ID gerado
    Task AtualizarAsync(T entidade); // atualiza alterações de um registro existente
    Task RemoverAsync(int id); // exclui um registro por ID
}