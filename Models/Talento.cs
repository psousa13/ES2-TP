using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class Talento
{
    public int IdTalento { get; set; }

    public int IdUtilizador { get; set; }

    public string PrimeiroNome { get; set; } = null!;

    public string Apelido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefone { get; set; }

    public string Pais { get; set; } = null!;

    public double? PrecoHora { get; set; }

    public string? Categoria { get; set; }

    public bool? Publico { get; set; }

    public virtual ICollection<Experiencia> Experiencia { get; set; } = new List<Experiencia>();

    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;

    public virtual ICollection<TalentoSkill> TalentoSkills { get; set; } = new List<TalentoSkill>();
}