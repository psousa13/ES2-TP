using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using TalentosIT.Web.Exceptions;

namespace TalentosIT.Web.Services
{
    public class TalentoSkillsService
    {
        private readonly TalentosItContext _context;
        private readonly RegistoAtividadeService _registoService;

        public TalentoSkillsService(TalentosItContext context, RegistoAtividadeService registoService)
        {
            _context = context;
            _registoService = registoService;
        }

        public async Task<(Talento Talento, List<Skill> SkillsDisponiveis)> GetDadosGestao(int? idTalento)
        {
            if (idTalento == null) throw new NotFoundException();

            var talento = await _context.Talentos
                .Include(t => t.TalentoSkills)
                .ThenInclude(ts => ts.IdSkillNavigation)
                .FirstOrDefaultAsync(t => t.IdTalento == idTalento);

            if (talento == null) throw new NotFoundException();

            var idsJaAssociados = talento.TalentoSkills.Select(ts => ts.IdSkill).ToHashSet();
            var skillsDisponiveis = await _context.Skills
                .Where(s => !idsJaAssociados.Contains(s.IdSkill))
                .OrderBy(s => s.Nome)
                .ToListAsync();

            return (talento, skillsDisponiveis);
        }

        public async Task AdicionarSkill(int idTalento, int idSkill, int anosExperiencia, int? userId)
        {
            if (anosExperiencia < 0)
            {
                throw new BusinessException("Erro", "Os anos de experiência não podem ser negativos.");
            }

            bool jaExiste = await _context.TalentoSkills
                .AnyAsync(ts => ts.IdTalento == idTalento && ts.IdSkill == idSkill);

            if (jaExiste)
            {
                throw new AlreadyRegisteredException();
            }

            var talentoSkill = new TalentoSkill
            {
                IdTalento = idTalento,
                IdSkill = idSkill,
                AnosExperiencia = anosExperiencia
            };

            _context.TalentoSkills.Add(talentoSkill);
            await _context.SaveChangesAsync();

            if (userId.HasValue)
            {
                await _registoService.RegistarAsync(
                    userId.Value, 
                    $"Skill (ID {idSkill}) adicionada ao talento (ID {idTalento}) com {anosExperiencia} anos de experiência."
                );
            }
        }

        public async Task EditarSkill(int idTalento, int idSkill, int anosExperiencia)
        {
            if (anosExperiencia < 0)
            {
                throw new BusinessException("Erro", "Os anos de experiência não podem ser negativos.");
            }

            var talentoSkill = await _context.TalentoSkills
                .FirstOrDefaultAsync(ts => ts.IdTalento == idTalento && ts.IdSkill == idSkill);

            if (talentoSkill == null) throw new NotFoundException();

            talentoSkill.AnosExperiencia = anosExperiencia;
            await _context.SaveChangesAsync();
        }

        public async Task RemoverSkill(int idTalento, int idSkill, int? userId)
        {
            var talentoSkill = await _context.TalentoSkills
                .FirstOrDefaultAsync(ts => ts.IdTalento == idTalento && ts.IdSkill == idSkill);

            if (talentoSkill != null)
            {
                _context.TalentoSkills.Remove(talentoSkill);
                await _context.SaveChangesAsync();

                if (userId.HasValue)
                {
                    await _registoService.RegistarAsync(
                        userId.Value,
                        $"Skill (ID {idSkill}) removida do talento (ID {idTalento})."
                    );
                }
            }
        }
    }
}