using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BibliotecaMVC.ViewModels;

public class EmprestimoFormViewModel
{
    [Required(ErrorMessage = "Selecione um usuário.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um usuário válido.")]
    [Display(Name = "Usuário (Leitor)")]
    public int UsuarioID { get; set; }

    [Required(ErrorMessage = "Selecione um exemplar disponível.")]
    [Range(1, int.MaxValue, ErrorMessage = "Selecione um exemplar válido.")]
    [Display(Name = "Exemplar Disponível")]
    public int ExemplarID { get; set; }

    public IEnumerable<SelectListItem> Usuarios { get; set; } = [];
    public IEnumerable<SelectListItem> Exemplares { get; set; } = [];
}
