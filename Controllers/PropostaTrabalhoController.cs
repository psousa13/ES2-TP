using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Controllers
{
    public class PropostaTrabalhoController : Controller
    {
        private readonly TalentosItContext _context;

        public PropostaTrabalhoController(TalentosItContext context)
        {
            _context = context;
        }

        // GET: PropostaTrabalho
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");

            var userId = int.Parse(userIdClaim.Value);

            var propostas = await _context.PropostaTrabalhos
                .Where(p => p.IdUtilizador == userId)
                .Include(p => p.IdClienteNavigation)
                .ToListAsync();

            return View(propostas);
        }

        // GET: PropostaTrabalho/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var propostaTrabalho = await _context.PropostaTrabalhos
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.IdUtilizadorNavigation)
                .FirstOrDefaultAsync(m => m.IdProposta == id);
                
            if (propostaTrabalho == null) return NotFound();

            return View(propostaTrabalho);
        }

        // GET: PropostaTrabalho/Create
        public IActionResult Create()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");

            var userId = int.Parse(userIdClaim.Value);
            var clientes = _context.Clientes.Where(c => c.IdUtilizador == userId).ToList();
            ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "PrimeiroNome");

            return View();
        }

        // POST: PropostaTrabalho/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCliente,Titulo,Categoria,HorasTotais,Descricao")] PropostaTrabalho proposta)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");

            proposta.IdUtilizador = int.Parse(userIdClaim.Value);

            ModelState.Remove("IdUtilizadorNavigation");
            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("IdUtilizador");

            if (ModelState.IsValid)
            {
                _context.Add(proposta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var userId = int.Parse(userIdClaim.Value);
            ViewData["IdCliente"] = new SelectList(_context.Clientes.Where(c => c.IdUtilizador == userId).ToList(), "IdCliente", "PrimeiroNome", proposta.IdCliente);
            return View(proposta);
        }

        // GET: PropostaTrabalho/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");

            var userId = int.Parse(userIdClaim.Value);
            var proposta = await _context.PropostaTrabalhos.FindAsync(id);

            if (proposta == null) return NotFound();
            if (proposta.IdUtilizador != userId) return Forbid(); // <-- ownership check

            var clientes = _context.Clientes.Where(c => c.IdUtilizador == userId).ToList();
            ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "PrimeiroNome", proposta.IdCliente);

            return View(proposta);
        }

        // POST: PropostaTrabalho/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProposta,IdUtilizador,IdCliente,Titulo,Categoria,HorasTotais,Descricao")] PropostaTrabalho propostaTrabalho)
        {
            if (id != propostaTrabalho.IdProposta) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");

            var userId = int.Parse(userIdClaim.Value);
            if (propostaTrabalho.IdUtilizador != userId) return Forbid(); // <-- ownership check

            ModelState.Remove("IdUtilizadorNavigation");
            ModelState.Remove("IdClienteNavigation");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(propostaTrabalho);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropostaTrabalhoExists(propostaTrabalho.IdProposta)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var clientes = _context.Clientes.Where(c => c.IdUtilizador == userId).ToList();
            ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "PrimeiroNome", propostaTrabalho.IdCliente);
            return View(propostaTrabalho);
        }

        // GET: PropostaTrabalho/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");

            var userId = int.Parse(userIdClaim.Value);

            var proposta = await _context.PropostaTrabalhos
                .Include(p => p.IdClienteNavigation)
                .FirstOrDefaultAsync(m => m.IdProposta == id);

            if (proposta == null) return NotFound();
            if (proposta.IdUtilizador != userId) return Forbid(); // <-- ownership check

            return View(proposta);
        }

        // POST: PropostaTrabalho/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");

            var userId = int.Parse(userIdClaim.Value);
            var proposta = await _context.PropostaTrabalhos.FindAsync(id);

            if (proposta == null) return NotFound();
            if (proposta.IdUtilizador != userId) return Forbid(); // <-- ownership check

            _context.PropostaTrabalhos.Remove(proposta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool PropostaTrabalhoExists(int id)
        {
            return _context.PropostaTrabalhos.Any(e => e.IdProposta == id);
        }
    }
}
