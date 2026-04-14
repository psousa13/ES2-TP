using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Controllers
{
    [Authorize]
    public class PropostaTrabalhoController : Controller
    {
        private readonly TalentosItContext _context;

        public PropostaTrabalhoController(TalentosItContext context)
        {
            _context = context;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private bool IsAdmin() => User.IsInRole("Admin");
        private bool IsClient() => User.IsInRole("GestorUtilizadores") || IsAdmin();

        public async Task<IActionResult> Index()
        {
            if (IsClient())
            {
                var propostas = IsAdmin()
                    ? await _context.PropostaTrabalhos.Include(p => p.IdClienteNavigation).ToListAsync()
                    : await _context.PropostaTrabalhos
                        .Where(p => p.IdUtilizador == GetUserId())
                        .Include(p => p.IdClienteNavigation)
                        .ToListAsync();
                return View(propostas);
            }
            else
            {
                var propostas = await _context.PropostaTrabalhos
                    .Include(p => p.IdClienteNavigation)
                    .Include(p => p.PropostaSkills)
                        .ThenInclude(ps => ps.IdSkillNavigation)
                    .ToListAsync();
                return View("IndexWorker", propostas);
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();
            var proposta = await _context.PropostaTrabalhos
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.IdUtilizadorNavigation)
                .Include(p => p.PropostaSkills)
                    .ThenInclude(ps => ps.IdSkillNavigation)
                .FirstOrDefaultAsync(m => m.IdProposta == id);
            if (proposta == null) return NotFound();
            return View(proposta);
        }

        [Authorize(Roles = "GestorUtilizadores,Admin")]
        public IActionResult Create()
        {
            if (IsAdmin())
            {
                var clientes = _context.Clientes
                    .Select(c => new { c.IdCliente, Nome = c.PrimeiroNome + " " + c.Apelido })
                    .ToList();
                ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "Nome");
                ViewData["ShowClientePicker"] = true;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GestorUtilizadores,Admin")]
        public async Task<IActionResult> Create([Bind("IdCliente,Titulo,Categoria,HorasTotais,Descricao")] PropostaTrabalho proposta)
        {
            ModelState.Remove("IdUtilizadorNavigation");
            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("IdUtilizador");
            ModelState.Remove("PropostaSkills");

            if (!IsAdmin())
            {
                var userId = GetUserId();
                var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdUtilizador == userId);
                if (cliente == null)
                {
                    ModelState.AddModelError("", "Perfil de cliente não encontrado. Por favor contacte o administrador.");
                    return View(proposta);
                }
                proposta.IdCliente = cliente.IdCliente;
                ModelState.Remove("IdCliente");
            }

            if (ModelState.IsValid)
            {
                proposta.IdUtilizador = GetUserId();
                _context.Add(proposta);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            if (IsAdmin())
            {
                var clientes = _context.Clientes
                    .Select(c => new { c.IdCliente, Nome = c.PrimeiroNome + " " + c.Apelido })
                    .ToList();
                ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "Nome", proposta.IdCliente);
                ViewData["ShowClientePicker"] = true;
            }
            return View(proposta);
        }

        [Authorize(Roles = "GestorUtilizadores,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();
            var proposta = await _context.PropostaTrabalhos.FindAsync(id);
            if (proposta == null) return NotFound();
            if (!IsAdmin() && proposta.IdUtilizador != GetUserId()) return Forbid();

            var userId = GetUserId();
            var clientes = _context.Clientes
                .Where(c => IsAdmin() || c.IdUtilizador == userId)
                .Select(c => new { c.IdCliente, Nome = c.PrimeiroNome + " " + c.Apelido })
                .ToList();
            ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "Nome", proposta.IdCliente);
            return View(proposta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GestorUtilizadores,Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("IdProposta,IdUtilizador,IdCliente,Titulo,Categoria,HorasTotais,Descricao")] PropostaTrabalho proposta)
        {
            if (id != proposta.IdProposta) return NotFound();
            if (!IsAdmin() && proposta.IdUtilizador != GetUserId()) return Forbid();

            ModelState.Remove("IdUtilizadorNavigation");
            ModelState.Remove("IdClienteNavigation");
            ModelState.Remove("PropostaSkills");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(proposta);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.PropostaTrabalhos.Any(e => e.IdProposta == proposta.IdProposta)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            var userId = GetUserId();
            var clientes = _context.Clientes
                .Where(c => IsAdmin() || c.IdUtilizador == userId)
                .Select(c => new { c.IdCliente, Nome = c.PrimeiroNome + " " + c.Apelido })
                .ToList();
            ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "Nome", proposta.IdCliente);
            return View(proposta);
        }

        [Authorize(Roles = "GestorUtilizadores,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();
            var proposta = await _context.PropostaTrabalhos
                .Include(p => p.IdClienteNavigation)
                .FirstOrDefaultAsync(m => m.IdProposta == id);
            if (proposta == null) return NotFound();
            if (!IsAdmin() && proposta.IdUtilizador != GetUserId()) return Forbid();
            return View(proposta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GestorUtilizadores,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proposta = await _context.PropostaTrabalhos.FindAsync(id);
            if (proposta == null) return NotFound();
            if (!IsAdmin() && proposta.IdUtilizador != GetUserId()) return Forbid();
            _context.PropostaTrabalhos.Remove(proposta);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
