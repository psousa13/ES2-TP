using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class PropostaTrabalho
{
    public int IdProposta { get; set; }

    public int IdUtilizador { get; set; }

    public int IdCliente { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Categoria { get; set; }

    public int? HorasTotais { get; set; }

    public string? Descricao { get; set; }

    public virtual Cliente IdClienteNavigation { get; set; } = null!;

    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;

    public virtual ICollection<PropostaSkill> PropostaSkills { get; set; } = new List<PropostaSkill>();
}
