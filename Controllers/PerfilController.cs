using System.Security.Claims;
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
    public async Task<IActionResult> Edit()
    {
        var idUtilizador = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var utilizador = await _context.Utilizadors
            .FirstOrDefaultAsync(u => u.IdUtilizador == idUtilizador);

        if (utilizador == null) return NotFound();

        var model = new EditProfileViewModel
        {
            PrimeiroNome = utilizador.PrimeiroNome,
            Apelido = utilizador.Apelido,
            Email = utilizador.Email,
            Telefone = utilizador.Telefone,
            IsCliente = utilizador.TipoUtilizador == TipoUtilizador.Cliente
        };

        if (model.IsCliente)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.IdUtilizador == idUtilizador);

            if (cliente != null)
            {
                model.Rua = cliente.Rua;
                model.NumPorta = cliente.NumPorta;
                model.Cidade = cliente.Cidade;
                model.Pais = cliente.Pais;
            }
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditProfileViewModel model)
    {
        var idUtilizador = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

        var utilizador = await _context.Utilizadors
            .FirstOrDefaultAsync(u => u.IdUtilizador == idUtilizador);

        if (utilizador == null) return NotFound();

        model.IsCliente = utilizador.TipoUtilizador == TipoUtilizador.Cliente;

        if (model.IsCliente)
        {
            if (string.IsNullOrWhiteSpace(model.Rua))
                ModelState.AddModelError("Rua", "A rua é obrigatória.");
            if (string.IsNullOrWhiteSpace(model.NumPorta))
                ModelState.AddModelError("NumPorta", "O número de porta é obrigatório.");
            if (string.IsNullOrWhiteSpace(model.Cidade))
                ModelState.AddModelError("Cidade", "A cidade é obrigatória.");
            if (string.IsNullOrWhiteSpace(model.Pais))
                ModelState.AddModelError("Pais", "O país é obrigatório.");
        }

        if (!ModelState.IsValid) return View(model);

        var hasher = new PasswordHasher<Utilizador>();
        if (hasher.VerifyHashedPassword(utilizador, utilizador.PalavraPasse, model.ConfirmarPalavraPasse)
            == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("ConfirmarPalavraPasse", "Palavra passe incorreta.");
            return View(model);
        }

        utilizador.PrimeiroNome = model.PrimeiroNome;
        utilizador.Apelido = model.Apelido;
        utilizador.Email = model.Email;
        utilizador.Telefone = model.Telefone;
        _context.Update(utilizador);

        if (model.IsCliente)
        {
            var cliente = await _context.Clientes
                .FirstOrDefaultAsync(c => c.IdUtilizador == idUtilizador);

            if (cliente != null)
            {
                cliente.PrimeiroNome = model.PrimeiroNome;
                cliente.Apelido = model.Apelido;
                cliente.Email = model.Email;
                cliente.Telefone = model.Telefone;
                cliente.Rua = model.Rua!;
                cliente.NumPorta = model.NumPorta!;
                cliente.Cidade = model.Cidade!;
                cliente.Pais = model.Pais!;
                _context.Update(cliente);
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }
}
