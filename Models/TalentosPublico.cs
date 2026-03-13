using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class TalentosPublico
{
    public int? IdTalento { get; set; }

    public int? IdUtilizador { get; set; }

    public string? PrimeiroNome { get; set; }

    public string? Apelido { get; set; }

    public string? Email { get; set; }

    public string? Telefone { get; set; }

    public double? PrecoHora { get; set; }

    public string? Categoria { get; set; }

    public bool? Publico { get; set; }
}
