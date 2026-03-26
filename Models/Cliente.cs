using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class Cliente
{
    public int IdCliente { get; set; }

    public int IdUtilizador { get; set; }

    public string PrimeiroNome { get; set; } = null!;

    public string Apelido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefone { get; set; }

    public string? Rua { get; set; }

    public string? NumPorta { get; set; }

    public string? Cidade { get; set; }

    public string? Pais { get; set; }

    public virtual Utilizador? IdUtilizadorNavigation { get; set; }

    public virtual ICollection<PropostaTrabalho> PropostaTrabalhos { get; set; } = new List<PropostaTrabalho>();
}
