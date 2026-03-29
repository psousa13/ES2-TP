using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TalentosIT.Web.Models;

public class RegisterViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string PrimeiroNome { get; set; }

    [Required]
    public string Apelido { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string PalavraPasse { get; set; }

    [Required]
    [DataType(DataType.Password)]
    [Compare("PalavraPasse", ErrorMessage = "As passwords não coincidem.")]
    public string ConfirmarPalavraPasse { get; set; }
}