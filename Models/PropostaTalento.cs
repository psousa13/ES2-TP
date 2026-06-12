using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class PropostaTalento
{
    public int IdProposta { get; set; }

    public int IdTalento { get; set; }

    public EstadoProposta Estado { get; set; }

    public DateTime? DataResposta { get; set; }

    public virtual PropostaTrabalho IdPropostaNavigation { get; set; } = null!;
    public virtual Talento IdTalentoNavigation { get; set; } = null!;
}
