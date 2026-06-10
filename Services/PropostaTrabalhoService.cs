using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Models;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Services.Matching;

namespace TalentosIT.Web.Services
{
    public class PropostaTrabalhoService
    {
        private readonly TalentosItContext _context;
        private readonly MatchingEngine _matchingEngine;

        public PropostaTrabalhoService(
            TalentosItContext context,
            MatchingEngine matchingEngine)
        {
            _context = context;
            _matchingEngine = matchingEngine;
        }

        public async Task<PropostaTrabalho?> GetProposta(int? id)
        {
            if (id == null) return null;

            return await _context.PropostaTrabalhos
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.IdUtilizadorNavigation)
                .Include(p => p.PropostaSkills)
                .ThenInclude(ps => ps.IdSkillNavigation)
                .FirstOrDefaultAsync(m => m.IdProposta == id);
        }

        public Task<List<PropostaTrabalho>> GetPropostas()
        {
            return _context.PropostaTrabalhos
                .Include(p => p.IdClienteNavigation)
                .Include(p => p.PropostaSkills)
                .ThenInclude(ps => ps.IdSkillNavigation)
                .ToListAsync();
        }

        public Task<List<PropostaTrabalho>> GetPropostasCliente(int idUtilizador, bool isAdmin)
        {
            if (isAdmin)
            {
                return _context.PropostaTrabalhos
                    .Include(p => p.IdClienteNavigation)
                    .ToListAsync();
            }

            return _context.PropostaTrabalhos
                .Where(p => p.IdUtilizador == idUtilizador)
                .Include(p => p.IdClienteNavigation)
                .ToListAsync();
        }

        public Task<List<object>> GetClientes(int idUtilizador, bool isAdmin)
        {
            var clientes =
                (isAdmin ? _context.Clientes : _context.Clientes.Where(p => p.IdUtilizador == idUtilizador))
                .Select(c => new
                {
                    c.IdCliente,
                    Nome = c.PrimeiroNome + " " + c.Apelido
                });

            return clientes.ToListAsync<object>();
        }

        public async Task Criar(CreatePropostaDTO dto, int idUtilizador, bool isAdmin)
        {
            if (!isAdmin)
            {
                var cliente = await _context.Clientes
                    .FirstOrDefaultAsync(c => c.IdUtilizador == idUtilizador);

                if (cliente == null)
                    throw new NotFoundException();

                dto.IdCliente = cliente.IdCliente;
            }

            var proposta = new PropostaTrabalho
            {
                IdCliente = dto.IdCliente,
                IdUtilizador = idUtilizador,
                Titulo = dto.Titulo,
                Categoria = dto.Categoria,
                HorasTotais = dto.HorasTotais,
                Descricao = dto.Descricao
            };

            _context.Add(proposta);
            await _context.SaveChangesAsync();
        }

        public async Task Editar(int id, EditPropostaDTO dto, int idUtilizador, bool isAdmin)
        {
            var proposta = await _context.PropostaTrabalhos.FirstOrDefaultAsync(
                p => p.IdProposta == id
            );

            if (proposta == null)
            {
                throw new NotFoundException();
            }

            if (!isAdmin && proposta.IdUtilizador != idUtilizador)
            {
                throw new NoPermissionException();
            }

            proposta.IdUtilizador = dto.IdUtilizador;
            proposta.IdCliente = dto.IdCliente;
            proposta.Titulo = dto.Titulo;
            proposta.Categoria = dto.Categoria;
            proposta.HorasTotais = dto.HorasTotais;
            proposta.Descricao = dto.Descricao;

            _context.Update(proposta);
            await _context.SaveChangesAsync();
        }

        public async Task Eliminar(int id)
        {
            var proposta = await _context.PropostaTrabalhos.FindAsync(id);

            if (proposta != null)
            {
                _context.PropostaTrabalhos.Remove(proposta);
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<Talento>> GetTalentosElegiveis(int id)
        {
            var proposta = await _context.PropostaTrabalhos
                .Include(p => p.PropostaSkills)
                    .ThenInclude(ps => ps.IdSkillNavigation)
                .Include(p => p.IdClienteNavigation)
                .FirstOrDefaultAsync(p => p.IdProposta == id);

            if (proposta == null)
            {
                throw new NotFoundException();
            }

            if (proposta.PropostaSkills == null || !proposta.PropostaSkills.Any())
            {
                throw new NoSkillsException();
            }

            List<Talento> todosTalentos = await _context.Talentos
                .Where(t => t.Publico)
                .Include(t => t.TalentoSkills)
                    .ThenInclude(ts => ts.IdSkillNavigation)
                .ToListAsync();

            var talentosElegiveis = todosTalentos
                .Where(talento => _matchingEngine.IsMatch(talento, proposta))
                .OrderBy(t => t.PrecoHora * (proposta.HorasTotais ?? 0))
                .ThenBy(t => t.PrimeiroNome)
                .ThenBy(t => t.Apelido)
                .ToList();

            return talentosElegiveis;
        }

        public Task<bool> Existe(int id)
        {
            return _context.PropostaTrabalhos.AnyAsync(e => e.IdProposta == id);
        }
    }
}
