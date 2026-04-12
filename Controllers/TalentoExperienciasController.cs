using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace TalentosIT.Web.Controllers
{
    public class TalentoExperienciasController : Controller
    {
        private readonly TalentosItContext _context;

        public TalentoExperienciasController(TalentosItContext context)
        {
            _context = context;
        }

        // GET: Experiencias/Gerir
        [Authorize]
        public async Task<IActionResult> Gerir(int? id)
        {
            if (id == null) return NotFound();

            var talento = await _context.Talentos
                .Include(t => t.Experiencia)
                .FirstOrDefaultAsync(t => t.IdTalento == id);

            if (talento == null) return NotFound();
            
            return View(talento);
        }

        // GET: Experiencias/Create
        [Authorize]
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null) return NotFound();

            var talento = await _context.Talentos
                .Include(t => t.Experiencia)
                .FirstOrDefaultAsync(t => t.IdTalento == id);

            if (talento == null) return NotFound();

            ViewData["Talento"] = talento;
            return View();
        }

        // POST: Experiencias/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTalento,Titulo,Empresa,AnoInicio,AnoFim")] Experiencia model)
        {
            var talento = await _context.Talentos
                .FirstOrDefaultAsync(t => t.IdTalento == model.IdTalento);
            if (talento == null) return NotFound();
            ViewData["Talento"] = talento;

            if (!ModelState.IsValid) return View(model);

            if (model.AnoFim < model.AnoInicio)
            {
                ModelState.AddModelError("AnoFim", "Ano de Fim deve ser igual ou superior ao ano de início.");
                return View(model);
            }

            var overlap = ValidarDatasExperiencia(model);
            if (overlap != null)
            {
                ModelState.AddModelError("AnoFim", "Período da experiência está sobreposto com o da experiência " + overlap.Titulo + ".");
                return View(model);
            }

            _context.Add(model);
            await _context.SaveChangesAsync();

            if (talento == null) return NotFound();

            ViewData["Talento"] = talento;

            return RedirectToAction(nameof(Gerir), new { id = model.IdTalento });
        }

        // GET: Experiencias/Edit
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var experiencia = await _context.Experiencias
                .Include(e => e.IdTalentoNavigation)
                .FirstOrDefaultAsync(e => e.IdExperiencia == id);
            if (experiencia == null) return NotFound();
            return View(experiencia);
        }

        // POST: Experiencias/Edit
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdExperiencia,IdTalento,Titulo,Empresa,AnoInicio,AnoFim")] Experiencia model)
        {
            if (id != model.IdExperiencia) return NotFound();

            if (!ModelState.IsValid) return View(model);

            if (model.AnoFim < model.AnoInicio)
            {
                ModelState.AddModelError("AnoFim", "Ano de Fim deve ser igual ou superior ao ano de início.");
                return View(model);
            }

            var overlap = ValidarDatasExperiencia(model);
            if (overlap != null)
            {
                ModelState.AddModelError("AnoFim", "Período da experiência está sobreposto com o da experiência " + overlap.Titulo + ".");
                return View(model);
            }

            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Gerir), new { id = model.IdTalento });
        }

        // GET: Experiencias/Delete
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            Experiencia? experiencia = await _context.Experiencias
                .FirstOrDefaultAsync(m => m.IdExperiencia == id);

            if (experiencia == null) return NotFound();

            return View(experiencia);
        }

        // POST: Experiencias/Delete
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var experiencia = await _context.Experiencias.FindAsync(id);
            if (experiencia == null) return NotFound();

             _context.Experiencias.Remove(experiencia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Gerir), new { id= experiencia.IdTalento });
        }

        private Experiencia? ValidarDatasExperiencia(Experiencia model)
        {
            return null;
        }
    }
}
