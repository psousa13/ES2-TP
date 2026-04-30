using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.DTO;
using TalentosIT.Web.Models;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.ViewModels;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace TalentosIT.Web.Services
{
    public class TalentosService
    {
        private readonly TalentosItContext _context;

        public TalentosService(TalentosItContext context)
        {
            _context = context;
        }

        public async Task<Talento?> GetTalento(int? id)
        {
            if (id == null) return null;
            return await _context.Talentos
                .Include(t => t.IdUtilizadorNavigation)
                .Include(t => t.TalentoSkills)
                .ThenInclude(ts => ts.IdSkillNavigation)
                .Include(t => t.Experiencia)
                .FirstOrDefaultAsync(m => m.IdTalento == id);
        }

        public Task<List<Talento>> GetTalentos(int id, bool isAdmin)
        {
            IQueryable<Talento> query = _context.Talentos.Include(t => t.IdUtilizadorNavigation);
            if (!isAdmin) query = query.Where(t => t.IdUtilizador == id);

            return query.ToListAsync();
        }

        public async Task Criar(CreateTalentoDTO dto)
        {
            var talento = new Talento()
            {
                IdUtilizador = dto.IdUtilizador,
                PrecoHora = dto.PrecoHora,
                Publico = dto.Publico,
                Categoria = dto.Categoria,
            };

            var utilizador = await _context.Utilizadors.FindAsync(talento.IdUtilizador);
            if (utilizador != null)
            {
                talento.PrimeiroNome = utilizador.PrimeiroNome;
                talento.Apelido = utilizador.Apelido;
                talento.Email = utilizador.Email;
            }
            else
            {
                talento.PrimeiroNome = "-";
                talento.Apelido = "-";
                talento.Email = "-";
            }

            if (string.IsNullOrWhiteSpace(talento.Pais)) talento.Pais = "-";
            if (string.IsNullOrWhiteSpace(talento.Categoria)) talento.Categoria = "Outro";
            talento.PrecoHora ??= 0;

            _context.Add(talento);
            await _context.SaveChangesAsync();
        }

        public async Task<TalentoFormViewModel> GetTalentoFormViewData()
        {
            var utilizadores = await _context.Utilizadors
                .Select(u => new SelectListItem()
                {
                    Value = u.IdUtilizador.ToString(),
                    Text = u.PrimeiroNome + " " + u.Apelido + " (" + u.Email + ")"
                })
                .ToListAsync();

            var categoriasDB = await _context.Talentos
                .Where(t => t.Categoria != null)
                .Select(t => t.Categoria!)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();

            var categoriasBase = new List<string> { "Developer", "Designer", "Product Manager", "Project Manager" };
            var todasCategorias = categoriasBase.Union(categoriasDB).OrderBy(c => c).ToList();

            return new TalentoFormViewModel()
            {
                Utilizadores = utilizadores,
                Categorias = todasCategorias
            };
        }

        public async Task Editar(int id, EditTalentoDTO dto)
        {
            var talento = await _context.Talentos.FindAsync(id);

            if (talento == null)
            {
                throw new NotFoundException();
            }

            var utilizador = await _context.Utilizadors.FindAsync(talento.IdUtilizador);
            talento.Email = utilizador?.Email ?? talento.Email;

            dto.PrecoHora ??= 0;

            talento.Telefone = dto.Telefone;
            talento.PrecoHora = dto.PrecoHora;
            talento.Publico = dto.Publico;

            _context.Update(talento);
            await _context.SaveChangesAsync();
        }

        public async Task Eliminar(int id)
        {
            var talento = await _context.Talentos.FindAsync(id);
            if (talento != null) _context.Talentos.Remove(talento);
            await _context.SaveChangesAsync();
        }

        public bool Existe(int id)
        {
            return _context.Talentos.Any(e => e.IdTalento == id);
        }

        public async Task<List<Cliente>> GetClientes(int? id)
        {
            if (id == null) return new();
            return await _context.Clientes.Where(c => c.IdUtilizador == id).ToListAsync();
        }

        public async Task AtribuirClienteAsync(int talentoId, int clienteId, string titulo, int horasTotais)
        {
            var talento = await _context.Talentos.FindAsync(talentoId);
            var cliente = await _context.Clientes.FindAsync(clienteId);

            if (talento == null || cliente == null) throw new NotFoundException();

            bool existe = await _context.PropostaTrabalhos.AnyAsync(p =>
                p.IdUtilizador == talento.IdUtilizador &&
                p.IdCliente == clienteId &&
                p.Titulo == titulo);

            if (existe) throw new AlreadyRegisteredException();

            var proposta = new PropostaTrabalho
            {
                IdUtilizador = talento.IdUtilizador,
                IdCliente = clienteId,
                Titulo = string.IsNullOrWhiteSpace(titulo)
                    ? $"Proposta - {talento.PrimeiroNome} {talento.Apelido}"
                    : titulo,
                Categoria = talento.Categoria ?? "Geral",
                HorasTotais = horasTotais,
                Descricao = $"Talento {talento.PrimeiroNome} {talento.Apelido} apresentado ao cliente."
            };

            _context.PropostaTrabalhos.Add(proposta);
            await _context.SaveChangesAsync();
        }

        public Task<List<Skill>> GetSkills()
        {
            return _context.Skills.ToListAsync();
        }

        public Task<List<Talento>> Buscar(HashSet<int> idSkills)
        {
            return _context.Talentos
                .Where(t => t.Publico && _context.TalentoSkills
                    .Where(ts => ts.IdTalento == t.IdTalento && idSkills.Contains(ts.IdSkill))
                    .Select(ts => ts.IdSkill)
                    .Distinct()
                    .Count() == idSkills.Count)
                .Include(t => t.IdUtilizadorNavigation)
                .OrderBy(t => t.IdUtilizador)
                .ToListAsync();
        }
    }
}