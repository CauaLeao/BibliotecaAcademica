using BibliotecaMVC.Models;

namespace BibliotecaMVC.ViewModels;

public class LivroCatalogoItemViewModel
{
    public required Livro Livro { get; init; }
    public int TotalExemplares { get; init; }
    public int ExemplaresDisponiveis { get; init; }
}

public class LivroCatalogoViewModel
{
    public string? Termo { get; init; }
    public int? CategoriaId { get; init; }
    public string? Disponibilidade { get; init; }
    public string Ordenacao { get; init; } = "titulo_asc";
    public IEnumerable<Categoria> Categorias { get; init; } = [];
    public IEnumerable<LivroCatalogoItemViewModel> Itens { get; init; } = [];
}
