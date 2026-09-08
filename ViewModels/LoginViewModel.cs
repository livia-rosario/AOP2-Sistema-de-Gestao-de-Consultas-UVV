using System.ComponentModel.DataAnnotations;
namespace GestaoConsultasUVV.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Informe sua senha.")]
    [StringLength(128)]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
