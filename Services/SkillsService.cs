using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.ViewModels;

namespace TalentosIT.Web.Services
{
    public class SkillsService
    {
        private readonly TalentosItContext _context;

        public SkillsService(TalentosItContext context)
        {
            _context = context;
        }

        public async Task<Skill?> GetSkill(int? id)
        {
            if (id == null) return null;
            return await _context.Skills.FirstOrDefaultAsync(m => m.IdSkill == id);
        }

        public Task<List<Skill>> GetSkills()
        {
            return _context.Skills.OrderBy(s => s.Nome).ToListAsync();
        }

        public async Task Criar(Skill skill)
        {
            _context.Add(skill);
            await _context.SaveChangesAsync();
        }

        public async Task Editar(Skill skill)
        {
            if (skill == null)
            {
                throw new NotFoundException();
            }

            _context.Update(skill);
            await _context.SaveChangesAsync();
        }

        public async Task Eliminar(int id)
        {
            if (
                _context.TalentoSkills.Any(ts => ts.IdSkill == id)
                || _context.PropostaSkills.Any(ps => ps.IdSkill == id)
            ) {
                throw new ObjectInUseException();
            }
            var skill = await _context.Skills.FindAsync(id);
            if (skill != null) _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();
        }

        public bool Existe(int id)
        {
            return _context.Skills.Any(e => e.IdSkill == id);
        }
    }
}