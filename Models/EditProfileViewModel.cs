using System.ComponentModel.DataAnnotations;

public class EditProfileViewModel
{
    [Required]
    [Display(Name = "Primeiro Nome")]
    public string PrimeiroNome { get; set; }

    [Required]
    [Display(Name = "Apelido")]
    public string Apelido { get; set; }

    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; }

    [Display(Name = "Telefone")]
    public string? Telefone { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Palavra Passe")]
    public string ConfirmarPalavraPasse { get; set; }

    // Cliente-only fields
    [Display(Name = "Rua")]
    public string? Rua { get; set; }

    [Display(Name = "Número de Porta")]
    public string? NumPorta { get; set; }

    [Display(Name = "Cidade")]
    public string? Cidade { get; set; }

    [Display(Name = "País")]
    public string? Pais { get; set; }

    public bool IsCliente { get; set; }
}
