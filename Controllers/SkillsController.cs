using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Controllers
{
    public class SkillsController : Controller
    {
        private readonly TalentosItContext _context;

        public SkillsController(TalentosItContext context)
        {
            _context = context;
        }

        // GET: Skills — visible to all authenticated users
        [Authorize]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Skills.OrderBy(s => s.Nome).ToListAsync());
        }

        // GET: Skills/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var skill = await _context.Skills.FirstOrDefaultAsync(m => m.IdSkill == id);
            if (skill == null) return NotFound();
            return View(skill);
        }

        // GET: Skills/Create — admin only
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Skills/Create — admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("IdSkill,Nome,AreaProfissional")] Skill skill)
        {
            if (ModelState.IsValid)
            {
                _context.Add(skill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        // GET: Skills/Edit/5 — admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return NotFound();
            return View(skill);
        }

        // POST: Skills/Edit/5 — admin only
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("IdSkill,Nome,AreaProfissional")] Skill skill)
        {
            if (id != skill.IdSkill) return NotFound();
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Skills.Any(e => e.IdSkill == skill.IdSkill)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        // GET: Skills/Delete/5 — admin only
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var skill = await _context.Skills.FirstOrDefaultAsync(m => m.IdSkill == id);
            if (skill == null) return NotFound();
            return View(skill);
        }

        // POST: Skills/Delete/5 — admin only
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill != null)
            {
                bool emUso = _context.TalentoSkills.Any(ts => ts.IdSkill == id)
                          || _context.PropostaSkills.Any(ps => ps.IdSkill == id);
                if (emUso)
                {
                    TempData["Erro"] = "Esta skill não pode ser eliminada pois está associada a talentos ou propostas.";
                    return RedirectToAction(nameof(Index));
                }
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
