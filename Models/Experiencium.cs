using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class Experiencium
{
    public int IdExperiencia { get; set; }

    public int IdTalento { get; set; }

    public string Titulo { get; set; } = null!;

    public string Empresa { get; set; } = null!;

    public int AnoInicio { get; set; }

    public int? AnoFim { get; set; }

    public virtual Talento IdTalentoNavigation { get; set; } = null!;
}
