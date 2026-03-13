using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class Skill
{
    public int IdSkill { get; set; }

    public string Nome { get; set; } = null!;

    public string? AreaProfissional { get; set; }

    public virtual ICollection<PropostaSkill> PropostaSkills { get; set; } = new List<PropostaSkill>();

    public virtual ICollection<TalentoSkill> TalentoSkills { get; set; } = new List<TalentoSkill>();
}
