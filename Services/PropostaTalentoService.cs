using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using TalentosIT.Web.Exceptions;

namespace TalentosIT.Web.Services
{
    public class PropostaTalentoService
    {
        private readonly TalentosItContext _context;

        public PropostaTalentoService(TalentosItContext context)
        {
            _context = context;
        }

        public async Task ConvidarTalentos(int idProposta, List<int> idsTalentos, int idUtilizadorAutenticado, bool isAdmin)
        {
            var proposta = await _context.PropostaTrabalhos
                .FirstOrDefaultAsync(p => p.IdProposta == idProposta);

            if (proposta == null)
            {
                System.Console.WriteLine("AA");
                throw new NotFoundException();
            }

            if (!isAdmin && proposta.IdUtilizador != idUtilizadorAutenticado)
            {
                System.Console.WriteLine("BB");
                throw new NoPermissionException();
            }

            System.Console.WriteLine("CC");

            foreach (var idTalento in idsTalentos)
            {
                var jaExiste = await _context.Set<PropostaTalento>()
                    .AnyAsync(pt => pt.IdProposta == idProposta && pt.IdTalento == idTalento);

                if (!jaExiste)
                {
                    var vinculacao = new PropostaTalento
                    {
                        IdProposta = idProposta,
                        IdTalento = idTalento,
                        Estado = EstadoProposta.Pendente
                    };
                    _context.Add(vinculacao);
                }
            }
            await _context.SaveChangesAsync();
        }

        public async Task ResponderConvite(int idProposta, int idTalento, EstadoProposta novoEstado, int idUtilizadorAutenticado)
        {
            if (novoEstado != EstadoProposta.Aceite && novoEstado != EstadoProposta.Rejeitada)
                throw new NoPermissionException();

            var vinculo = await _context.Set<PropostaTalento>()
                .Include(pt => pt.IdTalentoNavigation)
                .FirstOrDefaultAsync(pt => pt.IdProposta == idProposta && pt.IdTalento == idTalento);

            if (vinculo == null) throw new NotFoundException();

            if (vinculo.IdTalentoNavigation.IdUtilizador != idUtilizadorAutenticado)
                throw new NoPermissionException();

            vinculo.Estado = novoEstado;
            vinculo.DataResposta = DateTime.UtcNow;

            _context.Update(vinculo);
            await _context.SaveChangesAsync();
        }

        public Task<List<PropostaTalento>> GetConvitesPorTalento(int idUtilizadorAutenticado)
        {
            return _context.Set<PropostaTalento>()
                .Include(pt => pt.IdPropostaNavigation)
                .Where(pt => pt.IdTalentoNavigation.IdUtilizador == idUtilizadorAutenticado)
                .ToListAsync();
        }
    }
}