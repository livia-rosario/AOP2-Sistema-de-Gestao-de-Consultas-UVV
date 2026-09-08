using System.ComponentModel.DataAnnotations;

namespace GestaoConsultasUVV.Models;

public class Consulta
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Informe a especialidade.")]
    [StringLength(100, ErrorMessage = "A especialidade deve ter no máximo 100 caracteres.")]
    public string Especialidade { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe a data e o horário da consulta.")]
    [Display(Name = "Data e horário")]
    [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy HH:mm}")]
    public DateTime DataHora { get; set; }

    [Required(ErrorMessage = "Informe a descrição.")]
    [StringLength(1000, ErrorMessage = "A descrição deve ter no máximo 1000 caracteres.")]
    [Display(Name = "Descrição")]
    [DataType(DataType.MultilineText)]
    public string Descricao { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Informe um usuário válido.")]
    public int UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;
}
