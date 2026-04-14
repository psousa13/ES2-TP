using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TalentosItContext _context;

        public HomeController(ILogger<HomeController> logger, TalentosItContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Stats for all users
            ViewData["TotalTalentos"] = await _context.Talentos.CountAsync();
            ViewData["TotalSkills"] = await _context.Skills.CountAsync();
            ViewData["TotalClientes"] = await _context.Clientes.CountAsync();
            ViewData["TotalPropostas"] = await _context.PropostaTrabalhos.CountAsync();

            if (!User.Identity?.IsAuthenticated ?? true)
                return View();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdStr == null) return View();
            var userId = int.Parse(userIdStr);

            // WORKER: show job offers matching their skills
            if (User.IsInRole("Utilizador"))
            {
                // Get this worker's talento and its skills
                var talento = await _context.Talentos
                    .Include(t => t.TalentoSkills)
                    .FirstOrDefaultAsync(t => t.IdUtilizador == userId);

                if (talento != null)
                {
                    var mySkillIds = talento.TalentoSkills.Select(ts => ts.IdSkill).ToHashSet();
                    var mySkillYears = talento.TalentoSkills.ToDictionary(ts => ts.IdSkill, ts => ts.AnosExperiencia);

                    // Find propostas where worker meets ALL required skills with enough years
                    var allPropostas = await _context.PropostaTrabalhos
                        .Include(p => p.IdClienteNavigation)
                        .Include(p => p.PropostaSkills)
                            .ThenInclude(ps => ps.IdSkillNavigation)
                        .ToListAsync();

                    var matchingPropostas = allPropostas.Where(p =>
                        p.PropostaSkills.Any() &&
                        p.PropostaSkills.All(ps =>
                            mySkillIds.Contains(ps.IdSkill) &&
                            mySkillYears.GetValueOrDefault(ps.IdSkill) >= ps.AnosMinimosExperiencia
                        )
                    ).ToList();

                    ViewData["MatchingPropostas"] = matchingPropostas;
                    ViewData["TalentoId"] = talento.IdTalento;
                    ViewData["HasTalento"] = true;
                }
                else
                {
                    ViewData["HasTalento"] = false;
                }

                return View("IndexWorker");
            }

            // CLIENT: show workers eligible for each of their job offers
            if (User.IsInRole("GestorUtilizadores"))
            {
                var propostas = await _context.PropostaTrabalhos
                    .Where(p => p.IdUtilizador == userId)
                    .Include(p => p.IdClienteNavigation)
                    .Include(p => p.PropostaSkills)
                        .ThenInclude(ps => ps.IdSkillNavigation)
                    .ToListAsync();

                var allTalentos = await _context.Talentos
                    .Where(t => t.Publico == true)
                    .Include(t => t.TalentoSkills)
                    .ToListAsync();

                var propostaMatches = new Dictionary<int, List<Talento>>();
                foreach (var p in propostas)
                {
                    var elegíveis = allTalentos.Where(t =>
                        p.PropostaSkills.Any() &&
                        p.PropostaSkills.All(ps =>
                        {
                            var ts = t.TalentoSkills.FirstOrDefault(x => x.IdSkill == ps.IdSkill);
                            return ts != null && ts.AnosExperiencia >= ps.AnosMinimosExperiencia;
                        })
                    ).OrderBy(t => t.PrecoHora).ToList();
                    propostaMatches[p.IdProposta] = elegíveis;
                }

                ViewData["PropostaMatches"] = propostaMatches;
                ViewData["Propostas"] = propostas;
                return View("IndexClient");
            }

            // ADMIN: general dashboard with stats
            return View();
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() =>
            View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
