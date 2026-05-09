using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;

namespace TalentosIT.Web.Controllers
{
    [Authorize]
    public class PropostaSkillsController : Controller
    {
        private readonly TalentosItContext _context;
        private readonly RegistoAtividadeService _registoService;

        public PropostaSkillsController(TalentosItContext context, RegistoAtividadeService registoService)
        {
            _context = context;
            _registoService = registoService;
        }

        // GET: PropostaSkills/Gerir/5  (5 = IdProposta)
        public async Task<IActionResult> Gerir(int? id)
        {
            if (id == null) return NotFound();

            var proposta = await _context.PropostaTrabalhos
                .Include(p => p.PropostaSkills)
                    .ThenInclude(ps => ps.IdSkillNavigation)
                .Include(p => p.IdClienteNavigation)
                .FirstOrDefaultAsync(p => p.IdProposta == id);

            if (proposta == null) return NotFound();

            var idsJaAssociados = proposta.PropostaSkills.Select(ps => ps.IdSkill).ToHashSet();
            var skillsDisponiveis = await _context.Skills
                .Where(s => !idsJaAssociados.Contains(s.IdSkill))
                .OrderBy(s => s.Nome)
                .ToListAsync();

            ViewData["IdSkill"] = new SelectList(skillsDisponiveis, "IdSkill", "Nome");
            ViewData["Proposta"] = proposta;

            return View(proposta);
        }

        // POST: PropostaSkills/Adicionar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GestorUtilizadores,Admin")]
        public async Task<IActionResult> Adicionar(int idProposta, int idSkill, int anosMinimosExperiencia)
        {
            if (anosMinimosExperiencia < 0)
            {
                TempData["Erro"] = "O limiar mínimo de anos de experiência não pode ser negativo.";
                return RedirectToAction(nameof(Gerir), new { id = idProposta });
            }

            bool jaExiste = await _context.PropostaSkills
                .AnyAsync(ps => ps.IdProposta == idProposta && ps.IdSkill == idSkill);

            if (jaExiste)
            {
                TempData["Aviso"] = "Esta skill já está associada à proposta.";
                return RedirectToAction(nameof(Gerir), new { id = idProposta });
            }

            var propostaSkill = new PropostaSkill
            {
                IdProposta = idProposta,
                IdSkill = idSkill,
                AnosMinimosExperiencia = anosMinimosExperiencia
            };

            _context.PropostaSkills.Add(propostaSkill);
            await _context.SaveChangesAsync();

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            if (userIdClaim != null)
                await _registoService.RegistarAsync(int.Parse(userIdClaim.Value), $"Skill (ID {idSkill}) adicionada à proposta (ID {idProposta}) com mínimo de {anosMinimosExperiencia} anos.");

            TempData["Sucesso"] = "Skill adicionada à proposta com sucesso!";
            return RedirectToAction(nameof(Gerir), new { id = idProposta });
        }

        // POST: PropostaSkills/Editar
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GestorUtilizadores,Admin")]
        public async Task<IActionResult> Editar(int idProposta, int idSkill, int anosMinimosExperiencia)
        {
            if (anosMinimosExperiencia < 0)
            {
                TempData["Erro"] = "O limiar mínimo de anos de experiência não pode ser negativo.";
                return RedirectToAction(nameof(Gerir), new { id = idProposta });
            }

            var propostaSkill = await _context.PropostaSkills
                .FirstOrDefaultAsync(ps => ps.IdProposta == idProposta && ps.IdSkill == idSkill);

            if (propostaSkill == null) return NotFound();

            propostaSkill.AnosMinimosExperiencia = anosMinimosExperiencia;
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Limiar mínimo de experiência atualizado.";
            return RedirectToAction(nameof(Gerir), new { id = idProposta });
        }

        // POST: PropostaSkills/Remover
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "GestorUtilizadores,Admin")]
        public async Task<IActionResult> Remover(int idProposta, int idSkill)
        {
            var propostaSkill = await _context.PropostaSkills
                .FirstOrDefaultAsync(ps => ps.IdProposta == idProposta && ps.IdSkill == idSkill);

            if (propostaSkill != null)
            {
                _context.PropostaSkills.Remove(propostaSkill);
                await _context.SaveChangesAsync();

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null)
                    await _registoService.RegistarAsync(int.Parse(userIdClaim.Value), $"Skill (ID {idSkill}) removida da proposta (ID {idProposta}).");

                TempData["Sucesso"] = "Skill removida da proposta.";
            }

            return RedirectToAction(nameof(Gerir), new { id = idProposta });
        }
    }
}
