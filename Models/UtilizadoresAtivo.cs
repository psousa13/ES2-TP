using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class UtilizadoresAtivo
{
    public int? IdUtilizador { get; set; }

    public string? PrimeiroNome { get; set; }

    public string? Apelido { get; set; }

    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public string? PalavraPasse { get; set; }

    public bool? Ativo { get; set; }
}
