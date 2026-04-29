using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;

namespace TalentosIT.Web.Controllers
{
    public class SkillsController : Controller
    {
        private readonly SkillsService _service;

        public SkillsController(SkillsService service)
        {
            _service = service;
        }

        // GET: Skills — visible to all authenticated users
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _service.GetSkills());
        }

        // GET: Skills/Details/5
        [Authorize]
        public Task<IActionResult> Details(int? id)
        {
            return GetSkillOrNotFound(id);
        }

        // GET: Skills/Create - admin only
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Skills/Create - Admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("IdSkill,Nome,AreaProfissional")] Skill skill)
        {
            if (!ModelState.IsValid) return View(skill);
            await _service.Criar(skill);
            return RedirectToAction(nameof(Index));
        }

        // GET: Skills/Edit/5 — admin only
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Edit(int? id)
        {
            return GetSkillOrNotFound(id);
        }

        // POST: Skills/Edit/5 — admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("IdSkill,Nome,AreaProfissional")] Skill skill)
        {
            if (id != skill.IdSkill) return NotFound();
            if (!ModelState.IsValid) return View(skill);

            try
            {
                await _service.Editar(skill);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_service.Existe(skill.IdSkill)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Skills/Delete/5 — admin only
        [Authorize(Roles = "Admin")]
        public Task<IActionResult> Delete(int? id)
        {
            return GetSkillOrNotFound(id);
        }

        // POST: Skills/Delete/5 — admin only
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skill = await _service.GetSkill(id);
            if (skill == null) return RedirectToAction(nameof(Index));

            try
            {
                await _service.Eliminar(id);
            }
            catch (ObjectInUseException)
            {
                TempData["Erro"] = "Esta skill não pode ser eliminada pois está associada a talentos ou propostas.";
                return View(id);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> GetSkillOrNotFound(int? id)
        {
            var skill = await _service.GetSkill(id);
            if (skill == null) return NotFound();
            return View(skill);
        }
    }
}