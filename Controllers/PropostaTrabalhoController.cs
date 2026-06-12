using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;

namespace TalentosIT.Web.Controllers
{
    [Authorize]
    public class PropostaTrabalhoController : Controller
    {
        private readonly PropostaTrabalhoService _service;
        private readonly RegistoAtividadeService _registoService;

        public PropostaTrabalhoController(PropostaTrabalhoService service, RegistoAtividadeService registoService)
        {
            _service = service;
            _registoService = registoService;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private bool IsAdmin() => User.IsInRole("Admin");
        private bool IsClient() => User.IsInRole("Cliente") || IsAdmin();

        public async Task<IActionResult> Index()
        {
            if (IsClient()) return View(await _service.GetPropostasCliente(GetUserId(), IsAdmin()));
            return View("IndexWorker", await _service.GetPropostas());
        }

        public async Task<IActionResult> Details(int? id)
        {
            var proposta = await _service.GetProposta(id);
            if (proposta == null) return NotFound();
            return View(proposta);
        }

        [Authorize(Roles = "Cliente,Admin")]
        public async Task<IActionResult> Create()
        {
            // Admin sees a client picker; Cliente and GestorUtilizadores create for themselves
            if (IsAdmin())
            {
                var clientes = await _service.GetClientes(GetUserId(), IsAdmin());
                ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "Nome");
                ViewData["ShowClientePicker"] = true;
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente,Admin")]
        public async Task<IActionResult> Create([Bind("IdCliente,Titulo,Categoria,HorasTotais,Descricao")] CreatePropostaDTO dto)
        {
            try
            {
                await _service.Criar(dto, GetUserId(), IsAdmin());
                await _registoService.RegistarAsync(GetUserId(), $"Proposta de trabalho criada: \"{dto.Titulo}\".");
                var criada = await _service.GetPropostaByTituloEUtilizador(dto.Titulo, GetUserId());
                return RedirectToAction("Gerir", "PropostaSkills", new { id = criada?.IdProposta });
            }
            catch (NotFoundException)
            {
                ModelState.AddModelError("", "Perfil de cliente não encontrado. Por favor contacte o administrador.");
                return View(dto);
            }
        }

        [Authorize(Roles = "Cliente,Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            var proposta = await _service.GetProposta(id);
            if (proposta == null) return NotFound();
            if (!IsAdmin() && proposta.IdUtilizador != GetUserId()) return Forbid();

            if (IsAdmin())
            {
                var clientes = await _service.GetClientes(GetUserId(), IsAdmin());
                ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "Nome", proposta.IdCliente);
            }

            EditPropostaDTO dto = new()
            {
                IdProposta = proposta.IdProposta,
                IdUtilizador = proposta.IdUtilizador,
                IdCliente = proposta.IdCliente,
                Titulo = proposta.Titulo,
                Categoria = proposta.Categoria,
                HorasTotais = proposta.HorasTotais,
                Descricao = proposta.Descricao
            };

            return View(dto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente,Admin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("IdProposta,IdUtilizador,IdCliente,Titulo,Categoria,HorasTotais,Descricao")] EditPropostaDTO dto)
        {
            if (!ModelState.IsValid)
            {
                if (IsAdmin())
                {
                    var clientes = await _service.GetClientes(GetUserId(), IsAdmin());
                    ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "Nome", dto.IdCliente);
                }
                return View(dto);
            }

            try
            {
                await _service.Editar(id, dto, GetUserId(), IsAdmin());
                await _registoService.RegistarAsync(GetUserId(), $"Proposta de trabalho (ID {id}) editada: \"{dto.Titulo}\".");
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException) { return NotFound(); }
            catch (NoPermissionException) { return Forbid(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _service.Existe(id)) return NotFound();
                throw;
            }
        }

        [Authorize(Roles = "Cliente,Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            var proposta = await _service.GetProposta(id);
            if (proposta == null) return NotFound();
            if (!IsAdmin() && proposta.IdUtilizador != GetUserId()) return Forbid();
            return View(proposta);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Cliente,Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var proposta = await _service.GetProposta(id);
            if (proposta == null) return NotFound();
            if (!IsAdmin() && proposta.IdUtilizador != GetUserId()) return Forbid();
            await _registoService.RegistarAsync(GetUserId(), $"Proposta de trabalho (ID {id}) \"{proposta.Titulo}\" eliminada.");
            await _service.Eliminar(id);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Elegiveis(int? id)
        {
            var proposta = await _service.GetProposta(id);
            if (proposta == null) return NotFound();

            try
            {
                List<Talento>? talentosElegiveis = await _service.GetTalentosElegiveis(proposta.IdProposta);
                ViewData["Proposta"] = proposta;
                return View(talentosElegiveis);
            }
            catch (NotFoundException) { return NotFound(); }
            catch (NoSkillsException)
            {
                ViewData["Aviso"] = "Esta proposta não tem skills exigidas definidas. Adicione skills antes de procurar talentos elegíveis.";
                ViewData["Proposta"] = proposta;
                return View(new List<Talento>());
            }
        }
    }
}
