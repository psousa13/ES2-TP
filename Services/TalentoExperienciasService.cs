using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using TalentosIT.Web.Exceptions;

namespace TalentosIT.Web.Services
{
    public class TalentoExperienciasService
    {
        private readonly TalentosItContext _context;

        public TalentoExperienciasService(TalentosItContext context)
        {
            _context = context;
        }

        public async Task<Talento> GetTalentoComExperiencias(int? id)
        {
            if (id == null) throw new NotFoundException();

            var talento = await _context.Talentos
                .Include(t => t.Experiencia)
                .FirstOrDefaultAsync(t => t.IdTalento == id);

            if (talento == null) throw new NotFoundException();

            return talento;
        }

        public async Task<Experiencia> GetExperiencia(int? id)
        {
            if (id == null) throw new NotFoundException();

            var experiencia = await _context.Experiencias
                .Include(e => e.IdTalentoNavigation)
                .FirstOrDefaultAsync(e => e.IdExperiencia == id);

            if (experiencia == null) throw new NotFoundException();

            return experiencia;
        }

        public async Task Criar(Experiencia model)
        {
            await GetTalentoComExperiencias(model.IdTalento);

            await ValidarData(model);

            _context.Add(model);
            await _context.SaveChangesAsync();
        }

        public async Task Editar(int id, Experiencia model)
        {
            if (id != model.IdExperiencia) throw new NotFoundException();

            await ValidarData(model);

            _context.Update(model);
            await _context.SaveChangesAsync();
        }

        public async Task Eliminar(int id)
        {
            var experiencia = await _context.Experiencias.FindAsync(id);
            if (experiencia == null) throw new NotFoundException();

            _context.Experiencias.Remove(experiencia);
            await _context.SaveChangesAsync();
        }

        private async Task ValidarData(Experiencia model)
        {
            if (model.AnoFim < model.AnoInicio)
            {
                throw new BusinessException("AnoFim", "O ano de fim deve ser igual ou superior ao ano de início.");
            }

            int anoAtual = DateTime.Now.Year;
            if (model.AnoInicio > anoAtual)
            {
                throw new BusinessException("AnoInicio", "O ano de início não pode ser no futuro.");
            }

            var overlap = await ValidarSobreposicao(model);
            if (overlap != null)
            {
                string fimStr = overlap.AnoFim.HasValue ? overlap.AnoFim.Value.ToString() : "Presente";
                throw new BusinessException("AnoInicio", $"O período sobrepõe-se com a experiência '{overlap.Titulo}' ({overlap.AnoInicio}–{fimStr}).");
            }
        }

        private Task<Experiencia?> ValidarSobreposicao(Experiencia model)
        {
            int novoInicio = model.AnoInicio;
            int? novoFim = model.AnoFim;

            return _context.Experiencias.FirstOrDefaultAsync(e =>
                e.IdTalento == model.IdTalento &&
                e.IdExperiencia != model.IdExperiencia &&
                e.AnoInicio < (novoFim ?? int.MaxValue) &&
                (e.AnoFim == null || e.AnoFim > novoInicio)
            );
        }
    }
}