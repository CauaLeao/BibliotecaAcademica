using BibliotecaMVC.Models;

namespace BibliotecaMVC.Repositories.Interfaces;

public interface IEmprestimoRepository : IRepository<Emprestimo>
{
    Task<IEnumerable<Emprestimo>> ObterPorUsuarioIdAsync(int usuarioId);
    Task<IEnumerable<Emprestimo>> ObterAtivosPorUsuarioIdAsync(int usuarioId);
    Task<IEnumerable<Emprestimo>> ObterAtivosAsync();
    Task<Emprestimo?> ObterAtivoPorExemplarIdAsync(int exemplarId);
}
