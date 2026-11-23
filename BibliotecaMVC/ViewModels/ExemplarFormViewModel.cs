using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Enums;

namespace BibliotecaMVC.ViewModels;

public class ExemplarFormViewModel
{
    public int ID { get; set; }
    public int LivroID { get; set; }
    public string LivroTitulo { get; set; } = string.Empty;

    [Required(ErrorMessage = "O código do exemplar é obrigatório.")]
    [Display(Name = "Código de identificação")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o estado de conservação.")]
    [Display(Name = "Estado de conservação")]
    public EstadoConservacao EstadoConservacao { get; set; } = EstadoConservacao.Novo;

    [Required(ErrorMessage = "Informe a data de aquisição.")]
    [DataType(DataType.Date)]
    [Display(Name = "Data de aquisição")]
    public DateTime DataAquisicao { get; set; } = DateTime.Today;

    public DisponibilidadeExemplar Disponibilidade { get; set; } = DisponibilidadeExemplar.Disponivel;
}
