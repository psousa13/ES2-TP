using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ClientesController : Controller
    {
        private readonly TalentosItContext _context;

        public ClientesController(TalentosItContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private bool IsAdmin() => User.IsInRole("Admin");

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var query = _context.Clientes.Include(c => c.IdUtilizadorNavigation).AsQueryable();
            if (!IsAdmin())
                query = query.Where(c => c.IdUtilizador == GetUserId());
            return View(await query.ToListAsync());
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes
                .Include(c => c.IdUtilizadorNavigation)
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null) return NotFound();
            if (!IsAdmin() && cliente.IdUtilizador != GetUserId()) return Forbid();
            return View(cliente);
        }

        // GET: Clientes/Create
        public IActionResult Create() => View();

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrimeiroNome,Apelido,Email,Telefone,Rua,NumPorta,Cidade,Pais")] Cliente cliente)
        {
            ModelState.Remove("IdUtilizadorNavigation");
            ModelState.Remove("PropostaTrabalhos");

            if (ModelState.IsValid)
            {
                cliente.IdUtilizador = GetUserId();
                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();
            if (!IsAdmin() && cliente.IdUtilizador != GetUserId()) return Forbid();
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCliente,IdUtilizador,PrimeiroNome,Apelido,Email,Telefone,Rua,NumPorta,Cidade,Pais")] Cliente cliente)
        {
            if (id != cliente.IdCliente) return NotFound();
            if (!IsAdmin() && cliente.IdUtilizador != GetUserId()) return Forbid();

            ModelState.Remove("IdUtilizadorNavigation");
            ModelState.Remove("PropostaTrabalhos");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Clientes.Any(e => e.IdCliente == cliente.IdCliente)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var cliente = await _context.Clientes
                .Include(c => c.IdUtilizadorNavigation)
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null) return NotFound();
            if (!IsAdmin() && cliente.IdUtilizador != GetUserId()) return Forbid();
            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null) return NotFound();
            if (!IsAdmin() && cliente.IdUtilizador != GetUserId()) return Forbid();
            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
