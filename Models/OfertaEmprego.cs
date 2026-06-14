using System;

namespace TalentosIT.Web.Models;

public class OfertaEmprego
{
    public int IdOferta { get; set; }

    public int IdProposta { get; set; }

    public int IdTalento { get; set; }

    public int IdClienteUtilizador { get; set; }

    public EstadoOferta Estado { get; set; } = EstadoOferta.Pendente;

    public DateTime DataEnvio { get; set; } = DateTime.UtcNow;

    public DateTime? DataResposta { get; set; }

    public virtual PropostaTrabalho IdPropostaNavigation { get; set; } = null!;

    public virtual Talento IdTalentoNavigation { get; set; } = null!;

    public virtual Utilizador IdClienteUtilizadorNavigation { get; set; } = null!;
}
