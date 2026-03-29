using System.ComponentModel.DataAnnotations;

public class EditProfileViewModel
{
    [Required]
    public string PrimeiroNome { get; set; }

    [Required]
    public string Apelido { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    public string? Telefone { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string ConfirmarPalavraPasse { get; set; }
}