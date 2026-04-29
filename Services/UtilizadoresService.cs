using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.Blazor;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Models;
using Microsoft.AspNetCore.Identity;
using TalentosIT.Web.Exceptions;

namespace TalentosIT.Web.Services
{
    public class UtilizadoresService
    {
        private readonly TalentosItContext _context;
        private readonly PasswordHasher<Utilizador> _hasher;

        public UtilizadoresService(TalentosItContext context)
        {
            _context = context;
            _hasher = new();
        }

        public async Task<Utilizador?> GetUtilizador(int? id)
        {
            if (id == null) return null;
            return await _context.Utilizadors.FirstOrDefaultAsync(m => m.IdUtilizador == id);
        }

        public Task<List<Utilizador>> GetUtilizadores()
        {
            return _context.Utilizadors.ToListAsync();
        }

        public async Task Criar(CreateUtilizadorDTO dto)
        {
            if (await _context.Utilizadors.AnyAsync(u => u.Email == dto.Email))
            {
                throw new AlreadyRegisteredException();
            }
            Utilizador utilizador = new()
            {
                PrimeiroNome = dto.PrimeiroNome,
                Apelido = dto.Apelido,
                Email = dto.Email,
                PalavraPasse = _hasher.HashPassword(null, dto.PalavraPasse),
                Telefone = dto.Telefone,
                TipoUtilizador = TipoUtilizador.Utilizador,
                Ativo = true
            };

            _context.Add(utilizador);
            await _context.SaveChangesAsync();
        }

        public async Task Editar(int id, EditUtilizadorDTO dto)
        {
            if (await _context.Utilizadors.AnyAsync(u => u.Email == dto.Email && u.IdUtilizador != dto.IdUtilizador))
            {
                throw new AlreadyRegisteredException();
            }

            var utilizador = await _context.Utilizadors.AsNoTracking().FirstOrDefaultAsync(
                u => u.IdUtilizador == id
            );

            if (utilizador == null)
            {
                throw new NotFoundException();
            }

            utilizador.PrimeiroNome = dto.PrimeiroNome;
            utilizador.Apelido = dto.Apelido;
            utilizador.Email = dto.Email;
            utilizador.Telefone = dto.Telefone;
            utilizador.TipoUtilizador = dto.TipoUtilizador;
            utilizador.Ativo = dto.Ativo;

            _context.Update(utilizador);
            await _context.SaveChangesAsync();
        }

        public async Task Eliminar(int id)
        {
            var utilizador = await _context.Utilizadors.FindAsync(id);
            if (utilizador != null) _context.Utilizadors.Remove(utilizador);
            await _context.SaveChangesAsync();
        }

        public Task<bool> Existe(int id)
        {
            return _context.Utilizadors.AnyAsync(e => e.IdUtilizador == id);
        }
    }
}
