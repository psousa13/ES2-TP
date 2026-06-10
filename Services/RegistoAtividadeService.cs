using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services
{
    public class RegistoAtividadeService
    {
        private readonly TalentosItContext _context;

        public RegistoAtividadeService(TalentosItContext context)
        {
            _context = context;
        }
        
        // NOVO — escreve um registo na BD
        public async Task RegistarAsync(int idUtilizador, string descricaoAcao)
        {
            var registo = new RegistoAtividade
            {
                IdUtilizador = idUtilizador,
                DataHora = DateTime.Now,
                DescricaoAcao = descricaoAcao
            };
            _context.RegistoAtividades.Add(registo);
            await _context.SaveChangesAsync();
        }

        // RF31 — Admin: todos os registos do sistema
        public async Task<List<RegistoAtividade>> GetTodos()
        {
            return await _context.RegistoAtividades
                .Include(r => r.IdUtilizadorNavigation)
                .OrderByDescending(r => r.DataHora)
                .ToListAsync();
        }

        // RF27 — Gestor: registos de um utilizador específico
        public async Task<List<RegistoAtividade>> GetPorUtilizador(int idUtilizador)
        {
            return await _context.RegistoAtividades
                .Include(r => r.IdUtilizadorNavigation)
                .Where(r => r.IdUtilizador == idUtilizador)
                .OrderByDescending(r => r.DataHora)
                .ToListAsync();
        }

        public async Task<List<Utilizador>> GetUtilizadores()
        {
            return await _context.Utilizadors.OrderBy(u => u.PrimeiroNome).ToListAsync();
        }
    }
}