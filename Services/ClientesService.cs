using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.ViewModels;

namespace TalentosIT.Web.Services
{
    public class ClientesService
    {
        private readonly TalentosItContext _context;

        public ClientesService(TalentosItContext context)
        {
            _context = context;
        }

        public async Task<Cliente?> GetCliente(int? id, int utilizadorId, bool isAdmin)
        {
            if (id == null)
            {
                throw new NotFoundException();
            }
            var cliente = await _context.Clientes
                .Include(c => c.IdUtilizadorNavigation)
                .FirstOrDefaultAsync(m => m.IdCliente == id);
            if (cliente == null)
            {
                throw new NotFoundException();
            }
            if (!isAdmin && cliente.IdUtilizador != utilizadorId)
            {
                throw new NoPermissionException();
            }
            return cliente;
        }

        public Task<List<Cliente>> GetClientes(int utilizadorId, bool isAdmin)
        {
            var query = _context.Clientes.Include(c => c.IdUtilizadorNavigation).AsQueryable();
            if (!isAdmin) query = query.Where(c => c.IdUtilizador == utilizadorId);
            return query.ToListAsync();
        }

        public async Task Criar(CreateClienteDTO dto)
        {
            var cliente = new Cliente()
            {
                IdCliente = dto.IdCliente,
                IdUtilizador = dto.IdUtilizador,
                PrimeiroNome = dto.PrimeiroNome,
                Apelido = dto.Apelido,
                Email = dto.Email,
                Telefone = dto.Telefone,
                Rua = dto.Rua,
                NumPorta = dto.NumPorta,
                Cidade = dto.Cidade,
                Pais = dto.Pais
            };
            _context.Add(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task Editar(int id, EditClienteDTO dto, int idUtilizador, bool isAdmin)
        {
            if (id != dto.IdCliente)
            {
                throw new NotFoundException();
            }
            if (!isAdmin && dto.IdUtilizador != idUtilizador)
            {
                throw new NoPermissionException();
            }

            var cliente = await _context.Clientes.FirstOrDefaultAsync(c => c.IdCliente == id);

            if (cliente == null)
            {
                throw new NotFoundException();
            }

            cliente.IdCliente = dto.IdCliente;
            cliente.IdUtilizador = dto.IdUtilizador;
            cliente.PrimeiroNome = dto.PrimeiroNome;
            cliente.Apelido = dto.Apelido;
            cliente.Email = dto.Email;
            cliente.Telefone = dto.Telefone;
            cliente.Rua = dto.Rua;
            cliente.NumPorta = dto.NumPorta;
            cliente.Cidade = dto.Cidade;
            cliente.Pais = dto.Pais;

            _context.Update(cliente);
            await _context.SaveChangesAsync();
        }

        public async Task Eliminar(int id, int idUtilizador, bool isAdmin)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
            {
                throw new NotFoundException();
            }
            if (!isAdmin && cliente.IdUtilizador != idUtilizador)
            {
                throw new NoPermissionException();
            }

            if (cliente != null) _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();
        }

        public bool Existe(int id)
        {
            return _context.Clientes.Any(e => e.IdCliente == id);
        }
    }
}