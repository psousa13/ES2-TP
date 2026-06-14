using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;

namespace TalentosIT.Web.Controllers;

[Authorize]
public class OfertasEmpregoController : Controller
{
    private readonly TalentosItContext _context;
    private readonly RegistoAtividadeService _registoService;

    public OfertasEmpregoController(TalentosItContext context, RegistoAtividadeService registoService)
    {
        _context = context;
        _registoService = registoService;
    }

    // POST: OfertasEmprego/Enviar
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Cliente,GestorUtilizadores,Admin")]
    public async Task<IActionResult> Enviar(int idProposta, int idTalento)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var proposta = await _context.PropostaTrabalhos.FindAsync(idProposta);
        if (proposta == null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        if (!isAdmin && proposta.IdUtilizador != userId) return Forbid();

        var talento = await _context.Talentos.FindAsync(idTalento);
        if (talento == null) return NotFound();

        var jaExiste = await _context.OfertasEmprego.AnyAsync(o =>
            o.IdProposta == idProposta &&
            o.IdTalento == idTalento &&
            o.Estado == EstadoOferta.Pendente);

        if (jaExiste)
        {
            TempData["Aviso"] = "Já existe uma oferta pendente para este talento nesta proposta.";
        }
        else
        {
            var oferta = new OfertaEmprego
            {
                IdProposta = idProposta,
                IdTalento = idTalento,
                IdClienteUtilizador = userId,
                Estado = EstadoOferta.Pendente,
                DataEnvio = DateTime.Now
            };
            _context.OfertasEmprego.Add(oferta);
            await _context.SaveChangesAsync();

            await _registoService.RegistarAsync(userId,
                $"Oferta de emprego enviada ao talento ID {idTalento} para a proposta ID {idProposta}.");

            TempData["Sucesso"] = "Oferta de emprego enviada com sucesso!";
        }

        return RedirectToAction("Buscar", "Talentos", new { propostaId = idProposta });
    }

    // POST: OfertasEmprego/Responder
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Utilizador")]
    public async Task<IActionResult> Responder(int idOferta, bool aceitar)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var oferta = await _context.OfertasEmprego
            .Include(o => o.IdTalentoNavigation)
            .FirstOrDefaultAsync(o => o.IdOferta == idOferta);

        if (oferta == null) return NotFound();

        if (oferta.IdTalentoNavigation.IdUtilizador != userId) return Forbid();

        if (oferta.Estado != EstadoOferta.Pendente)
        {
            TempData["Aviso"] = "Esta oferta já foi respondida.";
            return RedirectToAction(nameof(MinhasOfertas));
        }

        oferta.Estado = aceitar ? EstadoOferta.Aceite : EstadoOferta.Recusada;
        oferta.DataResposta = DateTime.Now;
        await _context.SaveChangesAsync();

        await _registoService.RegistarAsync(userId,
            $"Oferta ID {idOferta} {(aceitar ? "aceite" : "recusada")}.");

        TempData["Sucesso"] = aceitar ? "Oferta aceite com sucesso!" : "Oferta recusada.";
        return RedirectToAction(nameof(MinhasOfertas));
    }

    // GET: OfertasEmprego/MinhasOfertas (profissional)
    [Authorize(Roles = "Utilizador")]
    public async Task<IActionResult> MinhasOfertas()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var talento = await _context.Talentos.FirstOrDefaultAsync(t => t.IdUtilizador == userId);
        if (talento == null)
        {
            TempData["Aviso"] = "Não tem um perfil de talento criado.";
            return RedirectToAction("Index", "Home");
        }

        var ofertas = await _context.OfertasEmprego
            .Where(o => o.IdTalento == talento.IdTalento)
            .Include(o => o.IdPropostaNavigation)
                .ThenInclude(p => p.IdClienteNavigation)
            .Include(o => o.IdPropostaNavigation)
                .ThenInclude(p => p.PropostaSkills)
                    .ThenInclude(ps => ps.IdSkillNavigation)
            .OrderByDescending(o => o.DataEnvio)
            .ToListAsync();

        return View(ofertas);
    }

    // GET: OfertasEmprego/OfertasEnviadas (cliente)
    [Authorize(Roles = "Cliente,GestorUtilizadores,Admin")]
    public async Task<IActionResult> OfertasEnviadas()
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");

        var ofertas = await _context.OfertasEmprego
            .Where(o => isAdmin || o.IdClienteUtilizador == userId)
            .Include(o => o.IdPropostaNavigation)
                .ThenInclude(p => p.IdClienteNavigation)
            .Include(o => o.IdTalentoNavigation)
                .ThenInclude(t => t.IdUtilizadorNavigation)
            .OrderByDescending(o => o.DataEnvio)
            .ToListAsync();

        return View(ofertas);
    }

    // GET: OfertasEmprego/ContactoProfissional/5 (cliente, só se aceite)
    [Authorize(Roles = "Cliente,GestorUtilizadores,Admin")]
    public async Task<IActionResult> ContactoProfissional(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);
        var isAdmin = User.IsInRole("Admin");

        var oferta = await _context.OfertasEmprego
            .Include(o => o.IdTalentoNavigation)
                .ThenInclude(t => t.IdUtilizadorNavigation)
            .Include(o => o.IdPropostaNavigation)
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null) return NotFound();
        if (!isAdmin && oferta.IdClienteUtilizador != userId) return Forbid();
        if (oferta.Estado != EstadoOferta.Aceite) return Forbid();

        return View(oferta);
    }

    // GET: OfertasEmprego/ContactoCliente/5 (profissional, só se aceite)
    [Authorize(Roles = "Utilizador")]
    public async Task<IActionResult> ContactoCliente(int id)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)!.Value);

        var oferta = await _context.OfertasEmprego
            .Include(o => o.IdTalentoNavigation)
            .Include(o => o.IdPropostaNavigation)
                .ThenInclude(p => p.IdClienteNavigation)
            .Include(o => o.IdClienteUtilizadorNavigation)
            .FirstOrDefaultAsync(o => o.IdOferta == id);

        if (oferta == null) return NotFound();
        if (oferta.IdTalentoNavigation.IdUtilizador != userId) return Forbid();
        if (oferta.Estado != EstadoOferta.Aceite) return Forbid();

        return View(oferta);
    }
}