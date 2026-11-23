using BibliotecaMVC.Models;

namespace BibliotecaMVC.ViewModels;

public class LivroDetalhesViewModel
{
    public required Livro Livro { get; init; }
    public IEnumerable<Exemplar> Exemplares { get; init; } = [];
}
