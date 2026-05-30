using Microsoft.AspNetCore.Mvc;
using TalentosIT.Web.Models;
using Microsoft.AspNetCore.Authorization;
using TalentosIT.Web.Services;
using TalentosIT.Web.Exceptions;

namespace TalentosIT.Web.Controllers
{
    [Authorize]
    public class TalentoExperienciasController : Controller
    {
        private readonly TalentoExperienciasService _experienciasService;

        public TalentoExperienciasController(TalentoExperienciasService experienciasService)
        {
            _experienciasService = experienciasService;
        }

        // GET: TalentoExperiencias/Gerir/5
        public async Task<IActionResult> Gerir(int? id)
        {
            try
            {
                var talento = await _experienciasService.GetTalentoComExperiencias(id);
                return View(talento);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // GET: TalentoExperiencias/Create?id=5
        public async Task<IActionResult> Create(int? id)
        {
            try
            {
                var talento = await _experienciasService.GetTalentoComExperiencias(id);
                ViewData["Talento"] = talento;
                return View(new Experiencia { IdTalento = id!.Value });
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // POST: TalentoExperiencias/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdTalento,Titulo,Empresa,AnoInicio,AnoFim")] Experiencia model)
        {
            try
            {
                // Carrega o talento para a view caso falte alguma validação local ou do service
                var talento = await _experienciasService.GetTalentoComExperiencias(model.IdTalento);
                ViewData["Talento"] = talento;

                if (!ModelState.IsValid) return View(model);

                await _experienciasService.Criar(model);
                return RedirectToAction(nameof(Gerir), new { id = model.IdTalento });
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BusinessException e)
            {
                ModelState.AddModelError(e.Property, e.Message);
                return View(model);
            }
        }

        // GET: TalentoExperiencias/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                var experiencia = await _experienciasService.GetExperiencia(id);
                return View(experiencia);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // POST: TalentoExperiencias/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdExperiencia,IdTalento,Titulo,Empresa,AnoInicio,AnoFim")] Experiencia model)
        {
            if (!ModelState.IsValid) return View(model);

            try
            {
                await _experienciasService.Editar(id, model);
                return RedirectToAction(nameof(Gerir), new { id = model.IdTalento });
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (BusinessException e)
            {
                ModelState.AddModelError(e.Property, e.Message);
                return View(model);
            }
        }

        // GET: TalentoExperiencias/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                var experiencia = await _experienciasService.GetExperiencia(id);
                return View(experiencia);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }

        // POST: TalentoExperiencias/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var experiencia = await _experienciasService.GetExperiencia(id);
                int idTalento = experiencia.IdTalento;

                await _experienciasService.Eliminar(id);
                return RedirectToAction(nameof(Gerir), new { id = idTalento });
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
        }
    }
}