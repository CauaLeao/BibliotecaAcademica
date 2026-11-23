using System.ComponentModel.DataAnnotations;
using BibliotecaMVC.Enums;

namespace BibliotecaMVC.ViewModels;

public class UsuarioCriarViewModel
{
    [Required(ErrorMessage = "O nome é obrigatório.")]
    [Display(Name = "Nome completo")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "O CPF é obrigatório.")]
    [Display(Name = "CPF")]
    public string CPF { get; set; } = string.Empty;

    [Required(ErrorMessage = "O e-mail é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "A senha é obrigatória.")]
    [DataType(DataType.Password)]
    [Display(Name = "Senha")]
    public string Senha { get; set; } = string.Empty;

    [Display(Name = "Tipo de conta")]
    public TipoUsuario TipoUsuario { get; set; } = TipoUsuario.Cliente;

    // Campos específicos de Cliente
    [Display(Name = "Perfil")]
    public TipoCliente? Perfil { get; set; } = TipoCliente.Aluno;

    [Display(Name = "RA / Registro Institucional")]
    public string? RegistroInstitucional { get; set; }

    // Campo específico de Bibliotecário
    [Display(Name = "Matrícula funcional")]
    public string? Matricula { get; set; }

    // Controle visual: só Administrador vê a opção de cadastrar Bibliotecário
    public bool PodeCriarBibliotecario { get; set; }
}
