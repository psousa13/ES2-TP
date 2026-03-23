using Microsoft.AspNetCore.Mvc;
using TalentosIT.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace TalentosIT.Web.Controllers;

public class ContaController : Controller
{
    private readonly TalentosItContext _context;

    public ContaController(TalentosItContext context)
    {
        _context = context;
    }

    [HttpGet]
    [Route("SignUp")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost]
    [Route("SignUp")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (ModelState.IsValid)
        {
            if (await _context.Utilizadors.AnyAsync(u => u.Email == model.Email))
            {
                ModelState.AddModelError("Email", "Email já registado.");
                return View(model);
            }

            var hasher = new PasswordHasher<Utilizador>();
            var utilizador = new Utilizador
            {
                PrimeiroNome = model.PrimeiroNome,
                Apelido = model.Apelido,
                Email = model.Email,
                PalavraPasse = hasher.HashPassword(null, model.PalavraPasse),
                Ativo = true
            };

            _context.Utilizadors.Add(utilizador);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Home");
        }

        return View(model);
    }
}