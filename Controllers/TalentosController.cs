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

        public TalentosController(TalentosService service)
        {
            _service = service;
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
            await _service.Eliminar(id);
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
        public async Task<IActionResult> Buscar(HashSet<int>? idSkills)
        {
            ViewBag.Skills = new SelectList(await _service.GetSkills(), "IdSkill", "Nome");

            if (idSkills == null || idSkills.Count == 0)
            {
                TempData["Aviso"] = "Por favor seleciona uma Skill.";
                return View(new List<Talento>());
            }

            var results = await _service.Buscar(idSkills);

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