using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class TalentoSkill
{
    public int IdTalento { get; set; }

    public int IdSkill { get; set; }

    public int AnosExperiencia { get; set; }

    public virtual Skill IdSkillNavigation { get; set; } = null!;

    public virtual Talento IdTalentoNavigation { get; set; } = null!;
}
