using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TalentosIT.Web.Controllers
{
    [Authorize(Roles = "Admin,GestorUtilizadores")]
    public class ClientesController : Controller
    {
        private readonly ClientesService _service;

        public ClientesController(ClientesService service)
        {
            _service = service;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        private bool IsAdmin() => User.IsInRole("Admin");

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            var clientes = await _service.GetClientes(GetUserId(), IsAdmin());
            return View(clientes);
        }

        // GET: Clientes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                var cliente = await _service.GetCliente(id, GetUserId(), IsAdmin());
                if (cliente == null) return NotFound();
                return View(cliente);
            }
            catch (NoPermissionException)
            {
                return Forbid();
            }
        }

        // GET: Clientes/Create
        public IActionResult Create() => View();

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PrimeiroNome,Apelido,Email,Telefone,Rua,NumPorta,Cidade,Pais")] CreateClienteDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            dto.IdUtilizador = GetUserId();
            await _service.Criar(dto);
            return RedirectToAction(nameof(Index));
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            try
            {
                var cliente = await _service.GetCliente(id, GetUserId(), IsAdmin());
                EditClienteDTO dto = new()
                {
                    IdCliente = cliente.IdCliente,
                    IdUtilizador = cliente.IdUtilizador,
                    PrimeiroNome = cliente.PrimeiroNome,
                    Apelido = cliente.Apelido,
                    Email = cliente.Email,
                    Telefone = cliente.Telefone,
                    Rua = cliente.Rua,
                    NumPorta = cliente.NumPorta,
                    Cidade = cliente.Cidade,
                    Pais = cliente.Pais
                };
                return View(dto);
            }
            catch (NotFoundException)
            {
                if (IsAdmin())
            {
                var clientes = await _service.GetClientes(GetUserId(), IsAdmin());
                ViewData["IdCliente"] = new SelectList(clientes, "IdCliente", "Nome");
                ViewData["ShowClientePicker"] = true;
            }
                return NotFound();
            }
            catch (NoPermissionException)
            {
                return Forbid();
            }
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("IdCliente,IdUtilizador,PrimeiroNome,Apelido,Email,Telefone,Rua,NumPorta,Cidade,Pais")] EditClienteDTO dto)
        {
            if (!ModelState.IsValid) return View(dto);

            try
            {
                await _service.Editar(id, dto, GetUserId(), IsAdmin());
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (NoPermissionException)
            {
                return Forbid();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_service.Existe(id)) return NotFound();
                throw;
            }
        }

        // GET: Clientes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                return View(await _service.GetCliente(id, GetUserId(), IsAdmin()));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (NoPermissionException)
            {
                return Forbid();
            }
        }

        // POST: Clientes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _service.Eliminar(id, GetUserId(), IsAdmin());
                return RedirectToAction(nameof(Index));
            }
            catch (NotFoundException)
            {
                return NotFound();
            }
            catch (NoPermissionException)
            {
                return Forbid();
            }
        }
    }
}