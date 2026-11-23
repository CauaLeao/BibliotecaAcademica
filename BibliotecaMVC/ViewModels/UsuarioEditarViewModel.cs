using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Enums;

namespace BibliotecaMVC.ViewModels;

public class UsuarioEditarViewModel
{
    public int ID { get; set; }

    [Required(ErrorMessage = "O nome é obrigatório.")]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Display(Name = "CPF")]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Display(Name = "Nova senha (opcional)")]
    public string? NovaSenha { get; set; }

    public TipoUsuario TipoUsuario { get; set; }

    // Campos específicos de Cliente
    [Display(Name = "Perfil")]
    public TipoCliente? Perfil { get; set; }

    [Display(Name = "RA / Registro Institucional")]
    public string? RegistroInstitucional { get; set; }

    // Campo específico de Bibliotecário
    [Display(Name = "Matrícula funcional")]
    public string? Matricula { get; set; }
}
