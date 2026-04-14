using System.ComponentModel.DataAnnotations;

namespace TalentosIT.Web.Models;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Required]
    public string PrimeiroNome { get; set; } = null!;

    [Required]
    public string Apelido { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    public string PalavraPasse { get; set; } = null!;

    [Required]
    [DataType(DataType.Password)]
    [Compare("PalavraPasse", ErrorMessage = "As passwords não coincidem.")]
    public string ConfirmarPalavraPasse { get; set; } = null!;

    [Required]
    public string TipoUtilizador { get; set; } = "utilizador";

    // Address fields — only required for GestorUtilizadores, validated in controller
    public string? Rua { get; set; }
    public string? NumPorta { get; set; }
    public string? Cidade { get; set; }
    public string? Pais { get; set; }
}
