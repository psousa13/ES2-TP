using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace TalentosIT.Web.Controllers
{
    [Authorize]
    public class TalentoExperienciasController : Controller
    {
        private readonly TalentosItContext _context;

        public TalentoExperienciasController(TalentosItContext context)
        {
            _context = context;
        }

        // GET: TalentoExperiencias/Gerir/5
        public async Task<IActionResult> Gerir(int? id)
        {
            if (id == null) return NotFound();

            var talento = await _context.Talentos
                .Include(t => t.Experiencia)
                .FirstOrDefaultAsync(t => t.IdTalento == id);

            if (talento == null) return NotFound();

            return View(talento);
        }

        // GET: TalentoExperiencias/Create?id=5
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null) return NotFound();

            var talento = await _context.Talentos
                .Include(t => t.Experiencia)
                .FirstOrDefaultAsync(t => t.IdTalento == id);

            if (talento == null) return NotFound();

            ViewData["Talento"] = talento;
            return View(new Experiencia { IdTalento = id.Value });
        }

        // POST: TalentoExperiencias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTalento,Titulo,Empresa,AnoInicio,AnoFim")] Experiencia model)
        {
            var talento = await _context.Talentos
                .Include(t => t.Experiencia)
                .FirstOrDefaultAsync(t => t.IdTalento == model.IdTalento);

            if (talento == null) return NotFound();
            ViewData["Talento"] = talento;

            if (!ModelState.IsValid) return View(model);

            // FIX: validate AnoFim >= AnoInicio
            if (model.AnoFim.HasValue && model.AnoFim.Value < model.AnoInicio)
            {
                ModelState.AddModelError("AnoFim", "O ano de fim deve ser igual ou superior ao ano de início.");
                return View(model);
            }

            // FIX: validate year range makes sense
            int anoAtual = DateTime.Now.Year;
            if (model.AnoInicio > anoAtual)
            {
                ModelState.AddModelError("AnoInicio", "O ano de início não pode ser no futuro.");
                return View(model);
            }

            // FIX: corrected overlap detection
            var overlap = await ValidarSobreposicao(model);
            if (overlap != null)
            {
                ModelState.AddModelError("AnoInicio",
                    $"O período sobrepõe-se com a experiência '{overlap.Titulo}' ({overlap.AnoInicio}–{(overlap.AnoFim.HasValue ? overlap.AnoFim.Value.ToString() : "Presente")}).");
                return View(model);
            }

            _context.Add(model);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Gerir), new { id = model.IdTalento });
        }

        // GET: TalentoExperiencias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var experiencia = await _context.Experiencias
                .Include(e => e.IdTalentoNavigation)
                .FirstOrDefaultAsync(e => e.IdExperiencia == id);

            if (experiencia == null) return NotFound();
            return View(experiencia);
        }

        // POST: TalentoExperiencias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdExperiencia,IdTalento,Titulo,Empresa,AnoInicio,AnoFim")] Experiencia model)
        {
            if (id != model.IdExperiencia) return NotFound();

            if (!ModelState.IsValid) return View(model);

            // FIX: validate AnoFim >= AnoInicio
            if (model.AnoFim.HasValue && model.AnoFim.Value < model.AnoInicio)
            {
                ModelState.AddModelError("AnoFim", "O ano de fim deve ser igual ou superior ao ano de início.");
                return View(model);
            }

            int anoAtual = DateTime.Now.Year;
            if (model.AnoInicio > anoAtual)
            {
                ModelState.AddModelError("AnoInicio", "O ano de início não pode ser no futuro.");
                return View(model);
            }

            // FIX: corrected overlap detection
            var overlap = await ValidarSobreposicao(model);
            if (overlap != null)
            {
                ModelState.AddModelError("AnoInicio",
                    $"O período sobrepõe-se com a experiência '{overlap.Titulo}' ({overlap.AnoInicio}–{(overlap.AnoFim.HasValue ? overlap.AnoFim.Value.ToString() : "Presente")}).");
                return View(model);
            }

            _context.Update(model);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Gerir), new { id = model.IdTalento });
        }

        // GET: TalentoExperiencias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var experiencia = await _context.Experiencias
                .Include(e => e.IdTalentoNavigation)
                .FirstOrDefaultAsync(m => m.IdExperiencia == id);

            if (experiencia == null) return NotFound();

            return View(experiencia);
        }

        // POST: TalentoExperiencias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var experiencia = await _context.Experiencias.FindAsync(id);
            if (experiencia == null) return NotFound();

            _context.Experiencias.Remove(experiencia);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Gerir), new { id = experiencia.IdTalento });
        }

        // FIX: correct overlap logic
        // Two periods overlap if: startA <= endB AND startB <= endA
        // Treating null AnoFim as "present" (i.e., effectively infinity)
        private Task<Experiencia?> ValidarSobreposicao(Experiencia model)
        {
            int novoInicio = model.AnoInicio;
            int? novoFim = model.AnoFim;

            return _context.Experiencias.FirstOrDefaultAsync(e =>
                e.IdTalento == model.IdTalento &&
                e.IdExperiencia != model.IdExperiencia &&
                // existing starts before or when new ends (or new has no end)
                e.AnoInicio <= (novoFim ?? int.MaxValue) &&
                // existing ends after or when new starts (or existing has no end)
                (e.AnoFim == null || e.AnoFim >= novoInicio)
            );
        }
    }
}
