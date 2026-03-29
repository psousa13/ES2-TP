using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TalentosIT.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

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
        if (!ModelState.IsValid) return View(model);
        
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

        return RedirectToAction("Index", "Login");
    }

    [HttpGet]
    [Route("Login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("Login")]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var utilizador = await _context.Utilizadors.FirstOrDefaultAsync(u => u.Email == model.Email);

        if (utilizador == null)
        {
            ModelState.AddModelError("", "Email inválido.");
            return View(model);
        }

        var hasher = new PasswordHasher<Utilizador>();
        var resultado = hasher.VerifyHashedPassword(utilizador, utilizador.PalavraPasse, model.PalavraPasse);

        if (resultado == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError("PalavraPasse", "Palavra passe incorreta.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, utilizador.Email),
            new("IdUtilizador", utilizador.IdUtilizador.ToString())
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal
        );

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}