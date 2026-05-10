using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.ViewModels;

namespace TalentosIT.Web.Services
{
    public class RelatorioPrecoService
    {
        private readonly TalentosItContext _context;

        const int HORAS_POR_MES = 176;

        public RelatorioPrecoService(TalentosItContext context)
        {
            _context = context;
        }

        public async Task<List<RelatorioPrecoMensalDTO>> GetRelatorioPreco(TipoRelatorio tipo)
        {
            return tipo switch
            {
                TipoRelatorio.Pais => await GetRelatorioPais(),
                TipoRelatorio.Categoria => await GetRelatorioCategoria(),
                TipoRelatorio.Skills => await GetRelatorioSkills(),
                _ => [],
            };
        }

        private Task<List<RelatorioPrecoMensalDTO>> GetRelatorioPais()
        {
            return _context.Talentos
                .AsNoTracking()
                .GroupBy(t => t.Pais)
                .Select(g => new RelatorioPrecoMensalDTO
                {
                    Grupo = g.Key,

                    PrecoMensal = double.Round(
                        g.Average(t => (t.PrecoHora ?? 0) * HORAS_POR_MES),
                    2)
                })
                .OrderBy(x => x.Grupo)
                .ToListAsync();
        }

        private Task<List<RelatorioPrecoMensalDTO>> GetRelatorioCategoria()
        {
            return _context.Talentos
                .AsNoTracking()
                .GroupBy(t => t.Categoria)
                .Select(g => new RelatorioPrecoMensalDTO
                {
                    Grupo = g.Key ?? "Sem Categoria",

                    PrecoMensal = double.Round(
                        g.Average(t => (t.PrecoHora ?? 0) * HORAS_POR_MES),
                    2)
                })
                .OrderBy(x => x.Grupo)
                .ToListAsync();
        }

        private Task<List<RelatorioPrecoMensalDTO>> GetRelatorioSkills()
        {
            return _context.Talentos
                .AsNoTracking()
                .SelectMany(
                    t => t.TalentoSkills.Select(ts => new
                    {
                        Skill = ts.IdSkillNavigation.Nome,
                        PrecoMensal = (t.PrecoHora ?? 0) * HORAS_POR_MES
                    })
                )
                .GroupBy(t => t.Skill)
                .Select(g => new RelatorioPrecoMensalDTO
                {
                    Grupo = g.Key,

                    PrecoMensal = double.Round(
                        g.Average(x => x.PrecoMensal),
                    2)
                })
                .OrderBy(x => x.Grupo)
                .ToListAsync();
        }
    }
}