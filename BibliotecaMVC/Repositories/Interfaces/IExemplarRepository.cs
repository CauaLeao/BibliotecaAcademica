using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories.Interfaces;

public interface IExemplarRepository : IRepository<Exemplar>
{
    Task<Exemplar?> ObterPorCodigoAsync(string codigo);
    Task<IEnumerable<Exemplar>> ObterPorLivroIdAsync(int livroId);
    Task<IEnumerable<Exemplar>> ObterDisponiveisPorLivroIdAsync(int livroId);
    Task<bool> PossuiEmprestimosAsync(int exemplarId);
}
