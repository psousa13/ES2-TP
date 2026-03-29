using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

[Authorize]
public class PerfilController : Controller
{
    private readonly TalentosItContext _context;

    public PerfilController(TalentosItContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit()
    {
        String? idUtilizador = User.FindFirst("IdUtilizador")?.Value;
        if (idUtilizador == null)
        {
            return Unauthorized();
        }

        var utilizador = await _context.Utilizadors
            .FirstOrDefaultAsync(u => u.IdUtilizador == int.Parse(idUtilizador));

        if (utilizador == null)
        {
            return NotFound();
        }

        var model = new EditProfileViewModel
        {
            PrimeiroNome = utilizador.PrimeiroNome,
            Apelido = utilizador.Apelido,
            Email = utilizador.Email,
            Telefone = utilizador.Telefone
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        String? idUtilizador = User.FindFirst("IdUtilizador")?.Value;
        if (idUtilizador == null)
        {
            return Unauthorized();
        }

        var utilizador = await _context.Utilizadors
            .FirstOrDefaultAsync(u => u.IdUtilizador == int.Parse(idUtilizador));

        if (utilizador == null)
        {
            return NotFound();
        }

        var hasher = new PasswordHasher<Utilizador>();

        var resultado = hasher.VerifyHashedPassword(utilizador, utilizador.PalavraPasse, model.ConfirmarPalavraPasse);

        if (resultado == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("ConfirmarPalavraPasse", "Palavra passe incorreta.");
            return View(model);
        }

        utilizador.PrimeiroNome = model.PrimeiroNome;
        utilizador.Apelido = model.Apelido;
        utilizador.Email = model.Email;
        utilizador.Telefone = model.Telefone;

        _context.Update(utilizador);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }
}