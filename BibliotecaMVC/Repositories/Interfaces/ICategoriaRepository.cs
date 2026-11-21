using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories.Interfaces;

public interface ICategoriaRepository : IRepository<Categoria>
{
    Task<Categoria?> ObterPorNomeAsync(string nome);
    Task<bool> PossuiLivrosAsync(int categoriaId);
}
