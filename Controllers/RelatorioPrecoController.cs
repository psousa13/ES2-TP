using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;
using TalentosIT.Web.ViewModels;

namespace TalentosIT.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class RelatorioPrecoController : Controller
    {
        private readonly RelatorioPrecoService _service;

        public RelatorioPrecoController(RelatorioPrecoService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index(TipoRelatorio tipo = TipoRelatorio.None)
        {
            var resultados = await _service.GetRelatorioPreco(tipo);

            var vm = new RelatorioViewModel
            {
                Tipo = tipo,
                Resultados = resultados,
                Opcoes =
                [
                    new()
                    {
                        Value = nameof(TipoRelatorio.None),
                        Text = "— Seleciona o agrupamento —",
                        Selected = tipo == TipoRelatorio.None
                    },
                    new()
                    {
                        Value = nameof(TipoRelatorio.Categoria),
                        Text = "Categoria",
                        Selected = tipo == TipoRelatorio.Categoria
                    },
                    new()
                    {
                        Value = nameof(TipoRelatorio.Pais),
                        Text = "País",
                        Selected = tipo == TipoRelatorio.Pais
                    },
                    new()
                    {
                        Value = nameof(TipoRelatorio.Skills),
                        Text = "Skills",
                        Selected = tipo == TipoRelatorio.Skills
                    }
                ]
            };

            return View(vm);
        }
    }
}