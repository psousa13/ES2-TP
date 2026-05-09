using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalentosIT.Web.Services;

namespace TalentosIT.Web.Controllers
{
    [Authorize(Roles = "Admin,GestorUtilizadores")]
    public class RegistoAtividadeController : Controller
    {
        private readonly RegistoAtividadeService _service;

        public RegistoAtividadeController(RegistoAtividadeService service)
        {
            _service = service;
        }

        // RF31 — Admin: listar todos os registos do sistema
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var registos = await _service.GetTodos();
            return View(registos);
        }

        // RF27 — Gestor: listar registos de um utilizador específico
        [Authorize(Roles = "Admin,GestorUtilizadores")]
        public async Task<IActionResult> PorUtilizador(int? idUtilizador)
        {
            var utilizadores = await _service.GetUtilizadores();
            ViewData["Utilizadores"] = new SelectList(
                utilizadores.Select(u => new {
                    u.IdUtilizador,
                    NomeEmail = u.PrimeiroNome + " " + u.Apelido + " (" + u.Email + ")"
                }),
                "IdUtilizador",
                "NomeEmail",
                idUtilizador
            );

            if (idUtilizador == null)
                return View(new List<TalentosIT.Web.Models.RegistoAtividade>());

            var registos = await _service.GetPorUtilizador(idUtilizador.Value);
            return View(registos);
        }
    }
}