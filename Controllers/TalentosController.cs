using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using Microsoft.AspNetCore.Authorization;

namespace TalentosIT.Web.Controllers
{
    public class TalentosController : Controller
    {
        private readonly TalentosItContext _context;

        public TalentosController(TalentosItContext context)
        {
            _context = context;
        }

        // GET: Talentos
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var talentos = await _context.Talentos
                .Include(t => t.IdUtilizadorNavigation)
                .ToListAsync();
            return View(talentos);
        }

        // GET: Talentos/Details/5
        [Authorize]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var talento = await _context.Talentos
                .Include(t => t.IdUtilizadorNavigation)
                .Include(t => t.TalentoSkills)
                    .ThenInclude(ts => ts.IdSkillNavigation)
                .Include(t => t.Experiencia)
                .FirstOrDefaultAsync(m => m.IdTalento == id);

            if (talento == null)
                return NotFound();

            return View(talento);
        }

        // GET: Talentos/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await CarregarViewData();
            return View();
        }

        // POST: Talentos/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUtilizador,PrimeiroNome,Apelido,Email,Telefone,PrecoHora,Categoria,Publico,Pais")] Talento talento)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");
            talento.IdUtilizador = int.Parse(userIdClaim.Value);

            var utilizador = await _context.Utilizadors.FindAsync(talento.IdUtilizador);
            if (utilizador != null)
            {
                talento.PrimeiroNome = utilizador.PrimeiroNome;
                talento.Apelido = utilizador.Apelido;
                talento.Email = utilizador.Email; // FIX: use the real login email
            }
            else
            {
                talento.PrimeiroNome = "-";
                talento.Apelido = "-";
                talento.Email = "-";
            }

            if (string.IsNullOrWhiteSpace(talento.Pais)) talento.Pais = "-";
            if (string.IsNullOrWhiteSpace(talento.Categoria)) talento.Categoria = "Outro";
            talento.PrecoHora ??= 0;

            ModelState.Remove("PrimeiroNome");
            ModelState.Remove("Apelido");
            ModelState.Remove("Email");
            ModelState.Remove("Pais");
            ModelState.Remove("Categoria");
            ModelState.Remove("IdUtilizadorNavigation");

            if (ModelState.IsValid)
            {
                talento.Publico ??= false;
                _context.Add(talento);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await CarregarViewData(talento.IdUtilizador);
            return View(talento);
        }

        // GET: Talentos/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var talento = await _context.Talentos.FindAsync(id);
            if (talento == null)
                return NotFound();

            await CarregarViewData(talento.IdUtilizador);
            return View(talento);
        }

        // POST: Talentos/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTalento,IdUtilizador,PrimeiroNome,Apelido,Email,Telefone,PrecoHora,Categoria,Publico")] Talento talento)
        {
            if (id != talento.IdTalento)
                return NotFound();

            if (string.IsNullOrWhiteSpace(talento.PrimeiroNome)) talento.PrimeiroNome = "-";
            if (string.IsNullOrWhiteSpace(talento.Apelido)) talento.Apelido = "-";

            // FIX: always sync email from the utilizador instead of generating a placeholder
            var utilizador = await _context.Utilizadors.FindAsync(talento.IdUtilizador);
            talento.Email = utilizador?.Email ?? talento.Email;

            if (string.IsNullOrWhiteSpace(talento.Pais)) talento.Pais = "-";
            if (string.IsNullOrWhiteSpace(talento.Categoria)) talento.Categoria = "Outro";
            talento.PrecoHora ??= 0;

            ModelState.Remove("PrimeiroNome");
            ModelState.Remove("Apelido");
            ModelState.Remove("Email");
            ModelState.Remove("Pais");
            ModelState.Remove("Categoria");
            ModelState.Remove("IdUtilizadorNavigation");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(talento);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TalentoExists(talento.IdTalento))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await CarregarViewData(talento.IdUtilizador);
            return View(talento);
        }

        // GET: Talentos/Delete/5
        [Authorize]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var talento = await _context.Talentos
                .Include(t => t.IdUtilizadorNavigation)
                .FirstOrDefaultAsync(m => m.IdTalento == id);

            if (talento == null)
                return NotFound();

            return View(talento);
        }

        // POST: Talentos/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var talento = await _context.Talentos.FindAsync(id);
            if (talento != null)
                _context.Talentos.Remove(talento);

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // ---------------------------------------------------------------
        // RF11 - Atribuir Talento a Cliente
        // ---------------------------------------------------------------

        // GET: Talentos/AtribuirCliente/5
        [Authorize]
        public async Task<IActionResult> AtribuirCliente(int? id)
        {
            if (id == null)
                return NotFound();

            var talento = await _context.Talentos.FirstOrDefaultAsync(t => t.IdTalento == id);
            if (talento == null)
                return NotFound();

            var clientes = await _context.Clientes
                .Where(c => c.IdUtilizador == talento.IdUtilizador)
                .ToListAsync();

            if (!clientes.Any())
            {
                TempData["Aviso"] = "Não existem clientes associados a este utilizador. Crie um cliente primeiro.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewData["Talento"] = talento;
            ViewData["IdCliente"] = new SelectList(
                clientes.Select(c => new { c.IdCliente, NomeCompleto = c.PrimeiroNome + " " + c.Apelido }),
                "IdCliente",
                "NomeCompleto"
            );

            return View();
        }

        // POST: Talentos/AtribuirCliente/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtribuirCliente(int id, int idCliente, string titulo, int horasTotais)
        {
            var talento = await _context.Talentos.FindAsync(id);
            var cliente = await _context.Clientes.FindAsync(idCliente);

            if (talento == null || cliente == null)
                return NotFound();

            bool jaExiste = await _context.PropostaTrabalhos
                .AnyAsync(p => p.IdUtilizador == talento.IdUtilizador
                            && p.IdCliente == idCliente
                            && p.Titulo == titulo);

            if (jaExiste)
            {
                TempData["Aviso"] = "Já existe uma proposta com este título para este cliente.";
                return RedirectToAction(nameof(AtribuirCliente), new { id });
            }

            var proposta = new PropostaTrabalho
            {
                IdUtilizador = talento.IdUtilizador,
                IdCliente = idCliente,
                Titulo = string.IsNullOrWhiteSpace(titulo) ? $"Proposta - {talento.PrimeiroNome} {talento.Apelido}" : titulo,
                Categoria = talento.Categoria ?? "Geral",
                HorasTotais = horasTotais,
                Descricao = $"Talento {talento.PrimeiroNome} {talento.Apelido} apresentado ao cliente."
            };

            _context.PropostaTrabalhos.Add(proposta);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Talento apresentado ao cliente com sucesso!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // ---------------------------------------------------------------
        // Helper
        // ---------------------------------------------------------------

        private async Task CarregarViewData(int? idUtilizadorSelecionado = null)
        {
            var utilizadores = await _context.Utilizadors
                .Select(u => new {
                    u.IdUtilizador,
                    NomeEmail = u.PrimeiroNome + " " + u.Apelido + " (" + u.Email + ")"
                })
                .ToListAsync();

            ViewData["IdUtilizador"] = new SelectList(utilizadores, "IdUtilizador", "NomeEmail", idUtilizadorSelecionado);

            var categoriasDB = await _context.Talentos
                .Where(t => t.Categoria != null)
                .Select(t => t.Categoria!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var categoriasBase = new List<string> { "Developer", "Designer", "Product Manager", "Project Manager" };
            var todasCategorias = categoriasBase.Union(categoriasDB).OrderBy(c => c).ToList();

            ViewData["Categorias"] = todasCategorias
                .Select(c => new SelectListItem { Value = c, Text = c })
                .ToList();
        }

        private bool TalentoExists(int id)
        {
            return _context.Talentos.Any(e => e.IdTalento == id);
        }
    }
}