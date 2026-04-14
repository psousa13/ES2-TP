using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UtilizadoresController : Controller
    {
        private readonly TalentosItContext _context;

        public UtilizadoresController(TalentosItContext context)
        {
            _context = context;
        }

        // GET: Utilizadores
        public async Task<IActionResult> Index()
        {
            return View(await _context.Utilizadors.ToListAsync());
        }

        // GET: Utilizadores/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var utilizador = await _context.Utilizadors.FirstOrDefaultAsync(m => m.IdUtilizador == id);
            if (utilizador == null) return NotFound();
            return View(utilizador);
        }

        // GET: Utilizadores/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Utilizadores/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrimeiroNome,Apelido,Email,Telefone,PalavraPasse,TipoUtilizador,Ativo")] Utilizador utilizador)
        {
            if (await _context.Utilizadors.AnyAsync(u => u.Email == utilizador.Email))
            {
                ModelState.AddModelError("Email", "Email já registado.");
            }

            ModelState.Remove("Clientes");
            ModelState.Remove("PropostaTrabalhos");
            ModelState.Remove("RegistoAtividades");
            ModelState.Remove("Talentos");

            if (ModelState.IsValid)
            {
                var hasher = new PasswordHasher<Utilizador>();
                utilizador.PalavraPasse = hasher.HashPassword(null, utilizador.PalavraPasse);
                utilizador.Ativo ??= true;
                _context.Add(utilizador);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(utilizador);
        }

        // GET: Utilizadores/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var utilizador = await _context.Utilizadors.FindAsync(id);
            if (utilizador == null) return NotFound();
            return View(utilizador);
        }

        // POST: Utilizadores/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdUtilizador,PrimeiroNome,Apelido,Email,Telefone,TipoUtilizador,Ativo")] Utilizador utilizador)
        {
            if (id != utilizador.IdUtilizador) return NotFound();

            ModelState.Remove("PalavraPasse");
            ModelState.Remove("Clientes");
            ModelState.Remove("PropostaTrabalhos");
            ModelState.Remove("RegistoAtividades");
            ModelState.Remove("Talentos");

            if (ModelState.IsValid)
            {
                try
                {
                    // Keep existing password hash — don't overwrite it
                    var existing = await _context.Utilizadors.AsNoTracking()
                        .FirstOrDefaultAsync(u => u.IdUtilizador == id);
                    utilizador.PalavraPasse = existing?.PalavraPasse ?? utilizador.PalavraPasse;

                    _context.Update(utilizador);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Utilizadors.Any(e => e.IdUtilizador == utilizador.IdUtilizador))
                        return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(utilizador);
        }

        // GET: Utilizadores/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var utilizador = await _context.Utilizadors.FirstOrDefaultAsync(m => m.IdUtilizador == id);
            if (utilizador == null) return NotFound();
            return View(utilizador);
        }

        // POST: Utilizadores/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var utilizador = await _context.Utilizadors.FindAsync(id);
            if (utilizador != null)
                _context.Utilizadors.Remove(utilizador);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
