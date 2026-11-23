using BibliotecaMVC.Models;

namespace BibliotecaMVC.ViewModels;

public class UsuarioDetalhesViewModel
{
    public required Usuario Usuario { get; init; }
    public IEnumerable<Emprestimo> Emprestimos { get; init; } = [];
}
