using System.ComponentModel.DataAnnotations;

namespace GestaoConsultasUVV.Models;

public class Usuario
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe o nome.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    [StringLength(254, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
    [Display(Name = "E-mail")]
    public string Email { get; set; } = string.Empty;

    // Recebe o resultado do PasswordHasher; nunca a senha original.
    [Required]
    [StringLength(512)]
    public string SenhaHash { get; set; } = string.Empty;

    [Display(Name = "Data de cadastro")]
    public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

    public ICollection<Consulta> Consultas { get; set; } = new List<Consulta>();
}
