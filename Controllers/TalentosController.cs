using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using Microsoft.AspNetCore.Authorization;
using TalentosIT.Web.Services;

namespace TalentosIT.Web.Controllers
{
    public class TalentosController : Controller
    {
        private readonly TalentosService _service;
        private readonly RegistoAtividadeService _registoService;
        private readonly TalentosItContext _context;
        private readonly TalentosIT.Web.Services.Matching.MatchingEngine _matchingEngine;

        public TalentosController(TalentosService service, RegistoAtividadeService registoService, TalentosItContext context, TalentosIT.Web.Services.Matching.MatchingEngine matchingEngine)
        {
            _service = service;
            _registoService = registoService;
            _context = context;
            _matchingEngine = matchingEngine;
        }

        // GET: Talentos
        [Authorize]
        public async Task<IActionResult> Index()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");
            var id = int.Parse(userIdClaim.Value);

            var talentos = await _service.GetTalentos(id, User.IsInRole("Admin"));
            return View(talentos);
        }

        // GET: Talentos/Details/5
        [Authorize]
        public Task<IActionResult> Details(int? id)
        {
            return GetTalentoOrNotFound(id);
        }

        // GET: Talentos/Create
        [Authorize]
        public async Task<IActionResult> Create()
        {
            await LoadViewData();
            return View();
        }

        // POST: Talentos/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("IdUtilizador,PrecoHora,Categoria,Publico")] CreateTalentoDTO dto)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim == null) return RedirectToAction("Login", "Conta");
            dto.IdUtilizador = int.Parse(userIdClaim.Value);

            if (!ModelState.IsValid)
            {
                await LoadViewData(dto.IdUtilizador);
                return View(dto);
            }

            await _service.Criar(dto);
            await _registoService.RegistarAsync(dto.IdUtilizador, $"Perfil de talento criado. Categoria: {dto.Categoria}.");
            return RedirectToAction(nameof(Index));
        }

        // GET: Talentos/Edit/5
        [Authorize]
        public async Task<IActionResult> Edit(int? id)
        {
            var talento = await _service.GetTalento(id);
            if (talento == null) return NotFound();

            var dto = new EditTalentoDTO
            {
                IdTalento = talento.IdTalento,
                IdUtilizador = talento.IdUtilizador,
                Telefone = talento.Telefone,
                PrecoHora = talento.PrecoHora,
                Publico = talento.Publico
            };

            await LoadViewData(talento.IdUtilizador);
            return View(dto);
        }

        // POST: Talentos/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdTalento,IdUtilizador,Telefone,PrecoHora,Publico")] EditTalentoDTO dto)
        {
            if (id != dto.IdTalento) return NotFound();
            if (!ModelState.IsValid) {
                await LoadViewData(dto.IdUtilizador);
                return View(dto);
            }
            try
            {
                await _service.Editar(id, dto);
                await _registoService.RegistarAsync(dto.IdUtilizador, $"Perfil de talento (ID {id}) editado.");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_service.Existe(dto.IdTalento)) return NotFound();
                throw;
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Talentos/Delete/5
        [Authorize]
        public Task<IActionResult> Delete(int? id)
        {
            return GetTalentoOrNotFound(id);
        }

        // POST: Talentos/Delete/5
        [Authorize]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            await _service.Eliminar(id);
            if (userIdClaim != null)
                await _registoService.RegistarAsync(int.Parse(userIdClaim.Value), $"Perfil de talento (ID {id}) eliminado.");
            return RedirectToAction(nameof(Index));
        }

        // GET: Talentos/AtribuirCliente/5
        [Authorize]
        public async Task<IActionResult> AtribuirCliente(int? id)
        {
            var talento = await _service.GetTalento(id);
            if (talento == null) return NotFound();

            var clientes = await _service.GetClientes(talento.IdUtilizador);

            if (clientes.Count == 0)
            {
                TempData["Aviso"] = "Não existem clientes associados a este utilizador. Crie um cliente primeiro.";
                return RedirectToAction(nameof(Details), new { id });
            }

            ViewData["Talento"] = talento;
            ViewData["IdCliente"] = new SelectList(
                clientes.Select(c => new { c.IdCliente, NomeCompleto = c.PrimeiroNome + " " + c.Apelido }),
                "IdCliente", "NomeCompleto"
            );

            return View();
        }

        // POST: Talentos/AtribuirCliente/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AtribuirCliente(int id, int idCliente, string titulo, int horasTotais)
        {
            try
            {
                await _service.AtribuirCliente(id, idCliente, titulo, horasTotais);
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (AlreadyRegisteredException)
            {
                TempData["Aviso"] = "Já existe uma proposta com este título para este cliente.";
                return RedirectToAction(nameof(AtribuirCliente), new { id });
            }

            TempData["Sucesso"] = "Talento apresentado ao cliente com sucesso!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Talentos/Buscar
        [Authorize(Roles = "Cliente,GestorUtilizadores,Admin")]
        public async Task<IActionResult> Buscar(int? propostaId)
        {
            var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
            var isAdmin = User.IsInRole("Admin");

            var propostas = await _context.PropostaTrabalhos
                .Where(p => isAdmin || p.IdUtilizador == userId)
                .OrderBy(p => p.Titulo)
                .ToListAsync();

            ViewBag.Propostas = new SelectList(propostas, "IdProposta", "Titulo", propostaId);

            if (propostaId == null)
                return View(new List<Talento>());

            var proposta = await _context.PropostaTrabalhos
                .Include(p => p.PropostaSkills).ThenInclude(ps => ps.IdSkillNavigation)
                .Include(p => p.IdClienteNavigation)
                .FirstOrDefaultAsync(p => p.IdProposta == propostaId);

            if (proposta == null) return NotFound();
            if (!isAdmin && proposta.IdUtilizador != userId) return Forbid();

            if (proposta.PropostaSkills == null || !proposta.PropostaSkills.Any())
            {
                TempData["Aviso"] = "Esta proposta não tem skills definidas. Adicione skills à proposta antes de buscar talentos.";
                return View(new List<Talento>());
            }

            var todosTalentos = await _context.Talentos
                .Where(t => t.Publico)
                .Include(t => t.TalentoSkills).ThenInclude(ts => ts.IdSkillNavigation)
                .Include(t => t.IdUtilizadorNavigation)
                .ToListAsync();

            var results = todosTalentos
                .Where(t => _matchingEngine.IsMatch(t, proposta))
                .OrderBy(t => t.PrecoHora * (proposta.HorasTotais ?? 0))
                .ThenBy(t => t.PrimeiroNome)
                .ToList();

            return View(results);
        }

        // ---------------------------------------------------------------
        // Helper
        // ---------------------------------------------------------------

        private async Task<IActionResult> GetTalentoOrNotFound(int? id)
        {
            Talento? talento = await _service.GetTalento(id);
            if (talento == null) return NotFound();

            return View(talento);
        }

        private async Task LoadViewData(int? selectedUtilizadorId = null)
        {
            var vm = await _service.GetTalentoFormViewData();

            ViewData["IdUtilizador"] = new SelectList(vm.Utilizadores, "Value", "Text", selectedUtilizadorId);
            ViewData["Categorias"] = vm.Categorias.Select(c => new SelectListItem { Value = c, Text = c }).ToList();
        }
    }
}