using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class PropostaSkill
{
    public int IdProposta { get; set; }

    public int IdSkill { get; set; }

    public int AnosMinimosExperiencia { get; set; }

    public virtual PropostaTrabalho IdPropostaNavigation { get; set; } = null!;

    public virtual Skill IdSkillNavigation { get; set; } = null!;
}
