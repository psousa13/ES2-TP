using Microsoft.AspNetCore.Mvc.Rendering;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.ViewModels;

public class RelatorioViewModel
{
    public TipoRelatorio Tipo { get; set; }

    public List<SelectListItem> Opcoes { get; set; }

    public List<RelatorioPrecoMensalDTO> Resultados { get; set; }
}