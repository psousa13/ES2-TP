using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TalentosIT.Web.Services;
using System.Security.Claims;
using TalentosIT.Web.Models;
using TalentosIT.Web.Exceptions; // Para extrair o ID do Utilizador Autenticado

namespace TalentosIT.Web.Controllers
{
    [Authorize]
    public class PropostaTalentoController : Controller
    {
        private readonly PropostaTalentoService _propostaTalentoService;

        public PropostaTalentoController(PropostaTalentoService propostaTalentoService)
        {
            _propostaTalentoService = propostaTalentoService;
        }

        // GET: PropostaTalento/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");
            int idUtilizador = int.Parse(userIdClaim.Value);

            var convites = await _propostaTalentoService.GetConvitesPorTalento(idUtilizador);
            return View(convites);
        }

        // POST: PropostaTalento/Convidar
        [HttpPost]
        [Authorize(Roles = "Cliente,GestorUtilizadores,Admin")]
        public async Task<IActionResult> Convidar(int idProposta, List<int> idsTalentos)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");
            int idUtilizador = int.Parse(userIdClaim.Value);

            bool isAdmin = User.IsInRole("admin");

            try
            {
                await _propostaTalentoService.ConvidarTalentos(idProposta, idsTalentos, idUtilizador, isAdmin);
                TempData["Sucesso"] = "Talentos convidados com sucesso!";
                return RedirectToAction("Details", "PropostaTrabalho", new { id = idProposta });
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (NoPermissionException)
            {
                return Forbid();
            }
        }

        // POST: PropostaTalento/Responder
        [HttpPost]
        public async Task<IActionResult> Responder(int idProposta, int idTalento, EstadoProposta estado)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");
            int idUtilizador = int.Parse(userIdClaim.Value);

            try
            {
                await _propostaTalentoService.ResponderConvite(idProposta, idTalento, estado, idUtilizador);
                return RedirectToAction("MeusConvites");
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (NoPermissionException)
            {
                return Forbid();
            }
        }
    }
}