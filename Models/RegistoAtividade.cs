using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class RegistoAtividade
{
    public int IdRegisto { get; set; }

    public int IdUtilizador { get; set; }

    public DateTime? DataHora { get; set; }

    public string DescricaoAcao { get; set; } = null!;

    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;
}
