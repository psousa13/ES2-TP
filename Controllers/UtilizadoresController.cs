using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;
using System.Security.Claims;

namespace TalentosIT.Web.Controllers
{
    [Authorize(Roles = "Admin,GestorUtilizadores")]
    public class UtilizadoresController : Controller
    {
        private readonly UtilizadoresService _service;
        private readonly RegistoAtividadeService _registoService;

        public UtilizadoresController(
            UtilizadoresService service,
            RegistoAtividadeService registoService)
        {
            _service = service;
            _registoService = registoService;
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
        public async Task<IActionResult> Create([Bind("PrimeiroNome,Apelido,Email,Telefone,PalavraPasse,TipoUtilizador")] CreateUtilizadorDTO dto)
        {
            if (User.IsInRole("GestorUtilizadores") && dto.TipoUtilizador == TipoUtilizador.Admin)
            {
                ModelState.AddModelError("TipoUtilizador", "Um gestor não pode criar administradores.");
            }

            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.Criar(dto);

                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                await _registoService.RegistarAsync(
                    currentUserId,
                    $"Utilizador criado: {dto.Email}. Tipo: {dto.TipoUtilizador}."
                );
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

            if (User.IsInRole("GestorUtilizadores") &&
                utilizador.TipoUtilizador == TipoUtilizador.Admin)
            {
                return Forbid();
            }

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

            var utilizadorAtual = await _service.GetUtilizador(dto.IdUtilizador);

            if (utilizadorAtual == null) return NotFound();

            if (User.IsInRole("GestorUtilizadores") &&
                utilizadorAtual.TipoUtilizador == TipoUtilizador.Admin)
            {
                return Forbid();
            }

            if (User.IsInRole("GestorUtilizadores") && dto.TipoUtilizador == TipoUtilizador.Admin)
            {
                ModelState.AddModelError("TipoUtilizador", "Um gestor não pode promover utilizadores a administrador.");
            }

            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.Editar(id, dto);

                var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

                await _registoService.RegistarAsync(
                    currentUserId,
                    $"Utilizador (ID {dto.IdUtilizador}) editado. Novo tipo: {dto.TipoUtilizador}. Ativo: {dto.Ativo}."
                );
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _service.Existe(dto.IdUtilizador)) return NotFound();
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Utilizadores/Desativar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Desativar(int id)
        {
            var utilizador = await _service.GetUtilizador(id);

            if (utilizador == null) return NotFound();

            if (User.IsInRole("GestorUtilizadores") &&
                utilizador.TipoUtilizador == TipoUtilizador.Admin)
            {
                return Forbid();
            }

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            if (utilizador.IdUtilizador == currentUserId)
            {
                TempData["Erro"] = "Não pode desativar a sua própria conta.";
                return RedirectToAction(nameof(Index));
            }

            await _service.Desativar(id);

            await _registoService.RegistarAsync(
                currentUserId,
                $"Conta do utilizador (ID {utilizador.IdUtilizador}) desativada."
            );

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