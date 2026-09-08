using System.ComponentModel.DataAnnotations;
namespace GestaoConsultasUVV.ViewModels;

public class CadastroViewModel
{
    [Required(ErrorMessage = "Informe seu nome.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Use entre 2 e 100 caracteres.")]
    public string Nome { get; set; } = "";

    [Required(ErrorMessage = "Informe seu e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254)]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Crie uma senha.")]
    [StringLength(128, MinimumLength = 8, ErrorMessage = "Use entre 8 e 128 caracteres.")]
    [DataType(DataType.Password)]
    public string Senha { get; set; } = "";

    [Required(ErrorMessage = "Confirme sua senha.")]
    [Compare(nameof(Senha), ErrorMessage = "As senhas não coincidem.")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar senha")]
    public string ConfirmarSenha { get; set; } = "";
}
