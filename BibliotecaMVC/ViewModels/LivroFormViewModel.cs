using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaMVC.ViewModels;

public class LivroFormViewModel
{
    public int ID { get; set; }

    [Required(ErrorMessage = "O título é obrigatório.")]
    [Display(Name = "Título")]
    public string Titulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O autor é obrigatório.")]
    [Display(Name = "Autor")]
    public string Autor { get; set; } = string.Empty;

    [Required(ErrorMessage = "O ISBN é obrigatório.")]
    [Display(Name = "ISBN")]
    public string ISBN { get; set; } = string.Empty;

    [Required(ErrorMessage = "O resumo é obrigatório.")]
    [Display(Name = "Resumo")]
    public string Resumo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecione uma categoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione uma categoria válida.")]
    [Display(Name = "Categoria")]
    public int CategoriaID { get; set; }

    public IEnumerable<SelectListItem> Categorias { get; set; } = [];
}
