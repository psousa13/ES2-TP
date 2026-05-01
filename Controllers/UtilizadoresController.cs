using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;

namespace TalentosIT.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UtilizadoresController : Controller
    {
        private readonly UtilizadoresService _service;

        public UtilizadoresController(UtilizadoresService service)
        {
            _service = service;
        }

        // GET: Utilizadores
        public async Task<IActionResult> Index()
        {
            return View(await _service.GetUtilizadores());
        }

        // GET: Utilizadores/Details/5
        public Task<IActionResult> Details(int? id)
        {
            return GetUtilizadorOrNotFound(id);
        }

        // GET: Utilizadores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Utilizadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrimeiroNome,Apelido,Email,Telefone,PalavraPasse")] CreateUtilizadorDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.Criar(dto);
            }
            catch (AlreadyRegisteredException)
            {
                ModelState.AddModelError("Email", "Email já registado.");
                return View(dto);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Utilizadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var utilizador = await _service.GetUtilizador(id);

            if (utilizador == null) return NotFound();

            var dto = new EditUtilizadorDTO
            {
                IdUtilizador = utilizador.IdUtilizador,
                PrimeiroNome = utilizador.PrimeiroNome,
                Apelido = utilizador.Apelido,
                Email = utilizador.Email,
                Telefone = utilizador.Telefone,
                TipoUtilizador = utilizador.TipoUtilizador,
                Ativo = utilizador.Ativo
            };

            return View(dto);
        }

        // POST: Utilizadores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUtilizador,PrimeiroNome,Apelido,Email,Telefone,TipoUtilizador,Ativo")] EditUtilizadorDTO dto)
        {
            if (id != dto.IdUtilizador) return NotFound();

            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.Editar(id, dto);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _service.Existe(dto.IdUtilizador)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Utilizadores/Delete/5
        public Task<IActionResult> Delete(int? id)
        {
            return GetUtilizadorOrNotFound(id);
        }

        // POST: Utilizadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _service.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> GetUtilizadorOrNotFound(int? id)
        {
            Utilizador? utilizador = await _service.GetUtilizador(id);
            if (utilizador == null) return NotFound();

            return View(utilizador);
        }
    }
}