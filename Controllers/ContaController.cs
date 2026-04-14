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
        // If registering as client, validate address fields
        if (model.TipoUtilizador == "gestor_utilizadores")
        {
            if (string.IsNullOrWhiteSpace(model.Rua))
                ModelState.AddModelError("Rua", "A rua é obrigatória para clientes.");
            if (string.IsNullOrWhiteSpace(model.NumPorta))
                ModelState.AddModelError("NumPorta", "O número de porta é obrigatório para clientes.");
            if (string.IsNullOrWhiteSpace(model.Cidade))
                ModelState.AddModelError("Cidade", "A cidade é obrigatória para clientes.");
            if (string.IsNullOrWhiteSpace(model.Pais))
                ModelState.AddModelError("Pais", "O país é obrigatório para clientes.");
        }

        if (!ModelState.IsValid) return View(model);

        if (await _context.Utilizadors.AnyAsync(u => u.Email == model.Email))
        {
            ModelState.AddModelError("Email", "Email já registado.");
            return View(model);
        }

        var tipo = model.TipoUtilizador == "gestor_utilizadores"
            ? TipoUtilizador.GestorUtilizadores
            : TipoUtilizador.Utilizador;

        var hasher = new PasswordHasher<Utilizador>();
        var utilizador = new Utilizador
        {
            PrimeiroNome = model.PrimeiroNome,
            Apelido = model.Apelido,
            Email = model.Email,
            PalavraPasse = hasher.HashPassword(null!, model.PalavraPasse),
            TipoUtilizador = tipo,
            Ativo = true
        };

        _context.Utilizadors.Add(utilizador);
        await _context.SaveChangesAsync();

        // Auto-create Cliente record for GestorUtilizadores with full address
        if (tipo == TipoUtilizador.GestorUtilizadores)
        {
            var cliente = new Cliente
            {
                IdUtilizador = utilizador.IdUtilizador,
                PrimeiroNome = utilizador.PrimeiroNome,
                Apelido = utilizador.Apelido,
                Email = utilizador.Email,
                Telefone = utilizador.Telefone,
                Rua = model.Rua!,
                NumPorta = model.NumPorta!,
                Cidade = model.Cidade!,
                Pais = model.Pais!
            };
            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Login", "Conta");
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
            new(ClaimTypes.NameIdentifier, utilizador.IdUtilizador.ToString()),
            new(ClaimTypes.Role, utilizador.TipoUtilizador.ToString())
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
