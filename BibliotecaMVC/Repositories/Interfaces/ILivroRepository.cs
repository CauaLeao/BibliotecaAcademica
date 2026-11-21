using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories.Interfaces;

public interface ILivroRepository : IRepository<Livro>
{
    Task<Livro?> ObterPorIsbnAsync(string isbn);
    Task<IEnumerable<Livro>> BuscarAsync(string? termo, int? categoriaId);
    Task<bool> PossuiExemplaresAsync(int livroId);
}
