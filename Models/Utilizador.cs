using System;
using System.Collections.Generic;

namespace TalentosIT.Web.Models;

public partial class Utilizador
{
    public int IdUtilizador { get; set; }

    public string PrimeiroNome { get; set; } = null!;

    public string Apelido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefone { get; set; }

    public string PalavraPasse { get; set; } = null!;

    public bool? Ativo { get; set; }

    public virtual ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

    public virtual ICollection<PropostaTrabalho> PropostaTrabalhos { get; set; } = new List<PropostaTrabalho>();

    public virtual ICollection<RegistoAtividade> RegistoAtividades { get; set; } = new List<RegistoAtividade>();

    public virtual ICollection<Talento> Talentos { get; set; } = new List<Talento>();
}
