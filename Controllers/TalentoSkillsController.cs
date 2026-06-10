using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TalentosIT.Web.Services;
using TalentosIT.Web.Exceptions;
using System.Security.Claims;

namespace TalentosIT.Web.Controllers
{
    [Authorize]
    public class TalentoSkillsController : Controller
    {
        private readonly TalentoSkillsService _skillsService;

        public TalentoSkillsController(TalentoSkillsService skillsService)
        {
            _skillsService = skillsService;
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : null;
        }

        // GET: TalentoSkills/Gerir/5
        public async Task<IActionResult> Gerir(int? id)
        {
            try
            {
                var (talento, skillsDisponiveis) = await _skillsService.GetDadosGestao(id);

                ViewData["IdSkill"] = new SelectList(skillsDisponiveis, "IdSkill", "Nome");
                ViewData["Talento"] = talento;

                return View(talento);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // POST: TalentoSkills/Adicionar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(int idTalento, int idSkill, int anosExperiencia)
        {
            try
            {
                int? userId = GetUserId();
                await _skillsService.AdicionarSkill(idTalento, idSkill, anosExperiencia, userId);

                TempData["Sucesso"] = "Skill adicionada com sucesso!";
            }
            catch (AlreadyRegisteredException)
            {
                TempData["Aviso"] = "Esta skill já está associada ao perfil.";
            }
            catch (BusinessException e)
            {
                TempData[e.Property] = e.Message;
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Gerir), new { id = idTalento });
        }

        // POST: TalentoSkills/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int idTalento, int idSkill, int anosExperiencia)
        {
            try
            {
                await _skillsService.EditarSkill(idTalento, idSkill, anosExperiencia);
                TempData["Sucesso"] = "Anos de experiência atualizados.";
            }
            catch (BusinessException e)
            {
                TempData[e.Property] = e.Message;
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Gerir), new { id = idTalento });
        }

        // POST: TalentoSkills/Remover
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(int idTalento, int idSkill)
        {
            try
            {
                int? userId = GetUserId();
                await _skillsService.RemoverSkill(idTalento, idSkill, userId);
                TempData["Sucesso"] = "Skill removida do perfil.";
            }
            catch (NotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Gerir), new { id = idTalento });
        }
    }
}