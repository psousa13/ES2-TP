using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services.Matching;

namespace TalentosIT.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TalentosItContext _context;
        private readonly MatchingEngine _matchingEngine;

        public HomeController(
            ILogger<HomeController> logger,
            TalentosItContext context,
            MatchingEngine matchingEngine)
        {
            _logger = logger;
            _context = context;
            _matchingEngine = matchingEngine;
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
            if (userIdStr == null)
                return View();

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
                    // Find propostas where worker meets all matching rules
                    var allPropostas = await _context.PropostaTrabalhos
                        .Include(p => p.IdClienteNavigation)
                        .Include(p => p.PropostaSkills)
                            .ThenInclude(ps => ps.IdSkillNavigation)
                        .ToListAsync();

                    var matchingPropostas = allPropostas
                        .Where(proposta => _matchingEngine.IsMatch(talento, proposta))
                        .ToList();

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
            if (User.IsInRole("Cliente"))
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

                foreach (var proposta in propostas)
                {
                    var elegiveis = allTalentos
                        .Where(talento => _matchingEngine.IsMatch(talento, proposta))
                        .OrderBy(talento => talento.PrecoHora)
                        .ToList();

                    propostaMatches[proposta.IdProposta] = elegiveis;
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
