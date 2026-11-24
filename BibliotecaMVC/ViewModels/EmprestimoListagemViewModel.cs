using BibliotecaMVC.Models;

namespace BibliotecaMVC.ViewModels;

public class EmprestimoListagemViewModel
{
    public string? Situacao { get; set; }
    public string? Prazo { get; set; }
    public IEnumerable<Emprestimo> Emprestimos { get; set; } = [];
}
