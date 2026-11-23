using System.ComponentModel.DataAnnotations;

namespace BibliotecaMVC.ViewModels;

public class CategoriaViewModel
{
    public int ID { get; set; }

    [Required(ErrorMessage = "O nome da categoria é obrigatório.")]
    [Display(Name = "Nome")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "A descrição é obrigatória.")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = string.Empty;
}
