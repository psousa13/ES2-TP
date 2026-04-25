using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TalentosIT.Web.Models;

public partial class PropostaTrabalho
{
    [Key] public int IdProposta { get; set; }

    public int IdUtilizador { get; set; }

    [Display(Name = "Cliente")] public int IdCliente { get; set; }

    [Required(ErrorMessage = "O título é obrigatório")]
    public string Titulo { get; set; } = null!;

    public string? Categoria { get; set; }

    [Display(Name = "Horas Totais")] public int? HorasTotais { get; set; }

    [Display(Name = "Descrição")] public string? Descricao { get; set; }

    // As propriedades "virtual" são as relações que o EF usa
    public virtual Cliente IdClienteNavigation { get; set; } = null!;
    public virtual Utilizador IdUtilizadorNavigation { get; set; } = null!;
    public virtual ICollection<PropostaSkill> PropostaSkills { get; set; } = new List<PropostaSkill>();
}

