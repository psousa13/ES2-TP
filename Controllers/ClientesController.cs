using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using Microsoft.AspNetCore.Authorization;


namespace TalentosIT.Web.Controllers
{
    public class ClientesController : Controller
    {
        private readonly TalentosItContext _context;

        public ClientesController(TalentosItContext context)
        {
            _context = context;
        }

        // GET: Clientes
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");
            var idUtilizador = userIdClaim.Value;
            var talentosItContext = _context.Clientes
                .Where(c => c.IdUtilizador == int.Parse(idUtilizador))
                .Include(c => c.IdUtilizadorNavigation);
            return View(await talentosItContext.ToListAsync());
        }

        // GET: Clientes/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.IdUtilizadorNavigation)
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // GET: Clientes/Create
        [Authorize]
        public IActionResult Create()
        {
            ViewData["IdUtilizador"] = new SelectList(_context.Utilizadors, "IdUtilizador", "IdUtilizador");
            return View();
        }

        // POST: Clientes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Create([Bind("IdCliente,IdUtilizador,PrimeiroNome,Apelido,Email,Telefone,Rua,NumPorta,Cidade,Pais")] Cliente cliente)
        {
            if (ModelState.IsValid)
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return RedirectToAction("Login", "Conta");
                var idUtilizador = userIdClaim.Value;
                cliente.IdUtilizador = int.Parse(idUtilizador);

                _context.Add(cliente);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUtilizador"] = new SelectList(_context.Utilizadors, "IdUtilizador", "IdUtilizador", cliente.IdUtilizador);
            return View(cliente);
        }

        // GET: Clientes/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                return NotFound();
            }
            ViewData["IdUtilizador"] = new SelectList(_context.Utilizadors, "IdUtilizador", "IdUtilizador", cliente.IdUtilizador);
            return View(cliente);
        }

        // POST: Clientes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Edit(int id, [Bind("IdCliente,IdUtilizador,PrimeiroNome,Apelido,Email,Telefone,Rua,NumPorta,Cidade,Pais")] Cliente cliente)
        {
            if (id != cliente.IdCliente)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cliente);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ClienteExists(cliente.IdCliente))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["IdUtilizador"] = new SelectList(_context.Utilizadors, "IdUtilizador", "IdUtilizador", cliente.IdUtilizador);
            return View(cliente);
        }

        // GET: Clientes/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cliente = await _context.Clientes
                .Include(c => c.IdUtilizadorNavigation)
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                return NotFound();
            }

            return View(cliente);
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente != null)
            {
                _context.Clientes.Remove(cliente);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ClienteExists(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }
    }
}
