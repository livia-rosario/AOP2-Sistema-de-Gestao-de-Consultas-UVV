using System.ComponentModel.DataAnnotations;
namespace GestaoConsultasUVV.ViewModels;

public class ConsultaViewModel
{
    [Required(ErrorMessage = "Informe a especialidade.")]
    [StringLength(100, ErrorMessage = "Use no máximo 100 caracteres.")]
    public string Especialidade { get; set; } = "";

    [Required(ErrorMessage = "Informe a data e o horário.")]
    [Display(Name = "Data e horário")]
    public DateTime? DataHora { get; set; }

    [Required(ErrorMessage = "Informe uma descrição.")]
    [StringLength(1000, ErrorMessage = "Use no máximo 1.000 caracteres.")]
    [Display(Name = "Descrição")]
    public string Descricao { get; set; } = "";
}
