using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace TalentosIT.Web.Controllers;

public class ContaController : Controller
{
    private readonly IContaService _contaService;
    private readonly RegistoAtividadeService _registoService;

    public ContaController(IContaService contaService, RegistoAtividadeService registoService)
    {
        _contaService = contaService;
        _registoService = registoService;
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
        // If registering as cliente, validate address fields
        if (model.TipoUtilizador == "cliente")
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

        if (await _contaService.EmailExisteAsync(model.Email))
        {
            ModelState.AddModelError("Email", "Email já registado.");
            return View(model);
        }

        var tipo = model.TipoUtilizador == "cliente"
            ? TipoUtilizador.Cliente
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

        await _contaService.RegistarUtilizadorAsync(utilizador);

        // Auto-create Cliente record for Cliente accounts
        if (tipo == TipoUtilizador.Cliente)
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
            await _contaService.CriarClienteAsync(cliente);
        }

        var tipoLabel = tipo == TipoUtilizador.Cliente ? "Cliente" : "Profissional";
        await _registoService.RegistarAsync(utilizador.IdUtilizador, $"Conta criada. Tipo: {tipoLabel}.");
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

        var utilizador = await _contaService.ObterUtilizadorPorEmailAsync(model.Email);

        if (utilizador == null)
        {
            ModelState.AddModelError("", "Email inválido.");
            return View(model);
        }

        if (!utilizador.Ativo)
        {
            ModelState.AddModelError("", "Esta conta está desativada.");
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

        await _registoService.RegistarAsync(utilizador.IdUtilizador, "Login efetuado com sucesso.");

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (userIdClaim != null)
            await _registoService.RegistarAsync(int.Parse(userIdClaim.Value), "Logout efetuado.");

        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}
