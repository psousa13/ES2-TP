using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Controllers
{
    [Authorize]
    public class TalentoSkillsController : Controller
    {
        private readonly TalentosItContext _context;

        public TalentoSkillsController(TalentosItContext context)
        {
            _context = context;
        }

        // GET: TalentoSkills/Gerir/5  (5 = IdTalento)
        public async Task<IActionResult> Gerir(int? id)
        {
            if (id == null) return NotFound();

            var talento = await _context.Talentos
                .Include(t => t.TalentoSkills)
                    .ThenInclude(ts => ts.IdSkillNavigation)
                .FirstOrDefaultAsync(t => t.IdTalento == id);

            if (talento == null) return NotFound();

            // Skills ainda não associadas a este talento
            var idsJaAssociados = talento.TalentoSkills.Select(ts => ts.IdSkill).ToHashSet();
            var skillsDisponiveis = await _context.Skills
                .Where(s => !idsJaAssociados.Contains(s.IdSkill))
                .OrderBy(s => s.Nome)
                .ToListAsync();

            ViewData["IdSkill"] = new SelectList(skillsDisponiveis, "IdSkill", "Nome");
            ViewData["Talento"] = talento;

            return View(talento);
        }

        // POST: TalentoSkills/Adicionar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Adicionar(int idTalento, int idSkill, int anosExperiencia)
        {
            if (anosExperiencia < 0)
            {
                TempData["Erro"] = "Os anos de experiência não podem ser negativos.";
                return RedirectToAction(nameof(Gerir), new { id = idTalento });
            }

            bool jaExiste = await _context.TalentoSkills
                .AnyAsync(ts => ts.IdTalento == idTalento && ts.IdSkill == idSkill);

            if (jaExiste)
            {
                TempData["Aviso"] = "Esta skill já está associada ao perfil.";
                return RedirectToAction(nameof(Gerir), new { id = idTalento });
            }

            var talentoSkill = new TalentoSkill
            {
                IdTalento = idTalento,
                IdSkill = idSkill,
                AnosExperiencia = anosExperiencia
            };

            _context.TalentoSkills.Add(talentoSkill);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Skill adicionada com sucesso!";
            return RedirectToAction(nameof(Gerir), new { id = idTalento });
        }

        // POST: TalentoSkills/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int idTalento, int idSkill, int anosExperiencia)
        {
            if (anosExperiencia < 0)
            {
                TempData["Erro"] = "Os anos de experiência não podem ser negativos.";
                return RedirectToAction(nameof(Gerir), new { id = idTalento });
            }

            var talentoSkill = await _context.TalentoSkills
                .FirstOrDefaultAsync(ts => ts.IdTalento == idTalento && ts.IdSkill == idSkill);

            if (talentoSkill == null) return NotFound();

            talentoSkill.AnosExperiencia = anosExperiencia;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Anos de experiência atualizados.";
            return RedirectToAction(nameof(Gerir), new { id = idTalento });
        }

        // POST: TalentoSkills/Remover
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Remover(int idTalento, int idSkill)
        {
            var talentoSkill = await _context.TalentoSkills
                .FirstOrDefaultAsync(ts => ts.IdTalento == idTalento && ts.IdSkill == idSkill);

            if (talentoSkill != null)
            {
                _context.TalentoSkills.Remove(talentoSkill);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Skill removida do perfil.";
            }

            return RedirectToAction(nameof(Gerir), new { id = idTalento });
        }
    }
}
