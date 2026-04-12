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

        // GET: Skills
        public async Task<IActionResult> Index()
        {
            return View(await _context.Skills.ToListAsync());
        }

        // GET: Skills/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var skill = await _context.Skills.FirstOrDefaultAsync(m => m.IdSkill == id);
            if (skill == null) return NotFound();

            return View(skill);
        }

        // GET: Skills/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Skills/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdSkill,Nome,AreaProfissional")] Skill skill)
        {
            // FIX: check for duplicate name (unique constraint in DB)
            if (await _context.Skills.AnyAsync(s => s.Nome == skill.Nome))
            {
                ModelState.AddModelError("Nome", "Já existe uma skill com este nome.");
                return View(skill);
            }

            if (ModelState.IsValid)
            {
                _context.Add(skill);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        // GET: Skills/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var skill = await _context.Skills.FindAsync(id);
            if (skill == null) return NotFound();
            return View(skill);
        }

        // POST: Skills/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdSkill,Nome,AreaProfissional")] Skill skill)
        {
            if (id != skill.IdSkill) return NotFound();

            // FIX: check for duplicate name on edit (excluding itself)
            if (await _context.Skills.AnyAsync(s => s.Nome == skill.Nome && s.IdSkill != skill.IdSkill))
            {
                ModelState.AddModelError("Nome", "Já existe uma skill com este nome.");
                return View(skill);
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(skill);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SkillExists(skill.IdSkill)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(skill);
        }

        // GET: Skills/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var skill = await _context.Skills.FirstOrDefaultAsync(m => m.IdSkill == id);
            if (skill == null) return NotFound();

            // FIX: warn user if skill is in use
            bool emUso = await _context.TalentoSkills.AnyAsync(ts => ts.IdSkill == id)
                      || await _context.PropostaSkills.AnyAsync(ps => ps.IdSkill == id);

            ViewData["EmUso"] = emUso;

            return View(skill);
        }

        // POST: Skills/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // FIX: block delete if skill is associated to any talent or proposal (requirement RF2)
            bool emUso = await _context.TalentoSkills.AnyAsync(ts => ts.IdSkill == id)
                      || await _context.PropostaSkills.AnyAsync(ps => ps.IdSkill == id);

            if (emUso)
            {
                TempData["Erro"] = "Não é possível eliminar esta skill porque está associada a um ou mais perfis ou propostas.";
                return RedirectToAction(nameof(Delete), new { id });
            }

            var skill = await _context.Skills.FindAsync(id);
            if (skill != null)
            {
                _context.Skills.Remove(skill);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool SkillExists(int id)
        {
            return _context.Skills.Any(e => e.IdSkill == id);
        }
    }
}
