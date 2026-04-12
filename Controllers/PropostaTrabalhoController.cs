using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Controllers
{
    [Authorize] // FIX: entire controller requires login
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
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Conta");

            var propostas = await _context.PropostaTrabalhos
                .Where(p => p.IdUtilizador == userId)
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.PropostaSkills)
                .ToListAsync();

            return View(propostas);
        }

        // GET: PropostaTrabalho/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Conta");

            var proposta = await _context.PropostaTrabalhos
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.IdUtilizadorNavigation)
                .Include(p => p.PropostaSkills)
                    .ThenInclude(ps => ps.IdSkillNavigation)
                .FirstOrDefaultAsync(m => m.IdProposta == id);

            if (proposta == null) return NotFound();
            if (proposta.IdUtilizador != userId) return Forbid();

            return View(proposta);
        }

        // GET: PropostaTrabalho/Create
        public IActionResult Create()
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Conta");

            var clientes = _context.Clientes
                .Where(c => c.IdUtilizador == userId)
                .ToList();

            // FIX: show full name in dropdown, not just PrimeiroNome
            ViewData["IdCliente"] = new SelectList(
                clientes.Select(c => new { c.IdCliente, Nome = c.PrimeiroNome + " " + c.Apelido }),
                "IdCliente", "Nome");

            CarregarCategorias();
            return View();
        }

        // POST: PropostaTrabalho/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdCliente,Titulo,Categoria,HorasTotais,Descricao")] PropostaTrabalho proposta)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Conta");

            proposta.IdUtilizador = userId.Value;

            ModelState.Remove("IdUtilizadorNavigation");
            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("IdUtilizador");

            // FIX: validate client belongs to current user
            var clienteValido = await _context.Clientes
                .AnyAsync(c => c.IdCliente == proposta.IdCliente && c.IdUtilizador == userId);

            if (!clienteValido)
            {
                ModelState.AddModelError("IdCliente", "Cliente inválido.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(proposta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            var clientes = _context.Clientes.Where(c => c.IdUtilizador == userId).ToList();
            ViewData["IdCliente"] = new SelectList(
                clientes.Select(c => new { c.IdCliente, Nome = c.PrimeiroNome + " " + c.Apelido }),
                "IdCliente", "Nome", proposta.IdCliente);
            CarregarCategorias();
            return View(proposta);
        }

        // GET: PropostaTrabalho/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Conta");

            var proposta = await _context.PropostaTrabalhos.FindAsync(id);
            if (proposta == null) return NotFound();
            if (proposta.IdUtilizador != userId) return Forbid();

            var clientes = _context.Clientes.Where(c => c.IdUtilizador == userId).ToList();
            ViewData["IdCliente"] = new SelectList(
                clientes.Select(c => new { c.IdCliente, Nome = c.PrimeiroNome + " " + c.Apelido }),
                "IdCliente", "Nome", proposta.IdCliente);
            CarregarCategorias();
            return View(proposta);
        }

        // POST: PropostaTrabalho/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdProposta,IdUtilizador,IdCliente,Titulo,Categoria,HorasTotais,Descricao")] PropostaTrabalho proposta)
        {
            if (id != proposta.IdProposta) return NotFound();

            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Conta");
            if (proposta.IdUtilizador != userId) return Forbid();

            ModelState.Remove("IdUtilizadorNavigation");
            ModelState.Remove("IdClienteNavigation");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proposta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PropostaExists(proposta.IdProposta)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var clientes = _context.Clientes.Where(c => c.IdUtilizador == userId).ToList();
            ViewData["IdCliente"] = new SelectList(
                clientes.Select(c => new { c.IdCliente, Nome = c.PrimeiroNome + " " + c.Apelido }),
                "IdCliente", "Nome", proposta.IdCliente);
            CarregarCategorias();
            return View(proposta);
        }

        // GET: PropostaTrabalho/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Conta");

            var proposta = await _context.PropostaTrabalhos
                .Include(p => p.IdClienteNavigation)
                .FirstOrDefaultAsync(m => m.IdProposta == id);

            if (proposta == null) return NotFound();
            if (proposta.IdUtilizador != userId) return Forbid();

            return View(proposta);
        }

        // POST: PropostaTrabalho/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            if (userId == null) return RedirectToAction("Login", "Conta");

            var proposta = await _context.PropostaTrabalhos.FindAsync(id);
            if (proposta == null) return NotFound();
            if (proposta.IdUtilizador != userId) return Forbid();

            // FIX: remove associated PropostaSkills before deleting proposta
            var propostaSkills = _context.PropostaSkills.Where(ps => ps.IdProposta == id);
            _context.PropostaSkills.RemoveRange(propostaSkills);

            _context.PropostaTrabalhos.Remove(proposta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private int? GetUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier);
            return claim != null ? int.Parse(claim.Value) : null;
        }

        private void CarregarCategorias()
        {
            var categorias = new List<string> { "Developer", "Designer", "Product Manager", "Project Manager", "Outro" };
            ViewData["Categorias"] = categorias.Select(c => new SelectListItem { Value = c, Text = c }).ToList();
        }

        private bool PropostaExists(int id)
        {
            return _context.PropostaTrabalhos.Any(e => e.IdProposta == id);
        }
    }
}
