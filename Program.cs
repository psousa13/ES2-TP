using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;
using TalentosIT.Web.Services.Matching;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.MapEnum<TipoUtilizador>("tipo_utilizador");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<TalentosIT.Web.Models.TalentosItContext>(options => options.UseNpgsql(dataSource));

builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => options.LoginPath = "/Login");

builder.Services.AddScoped<UtilizadoresService>();
builder.Services.AddScoped<TalentosService>();
builder.Services.AddScoped<SkillsService>();
builder.Services.AddScoped<ClientesService>();
builder.Services.AddScoped<PropostaTrabalhoService>();
builder.Services.AddScoped<IContaService, ContaService>();
builder.Services.AddScoped<RegistoAtividadeService>();
builder.Services.AddScoped<RelatorioPrecoService>();
builder.Services.AddScoped<TalentoSkillsService>();
builder.Services.AddScoped<TalentoExperienciasService>();

builder.Services.AddScoped<IMatchingRule, ProposalHasSkillsMatchingRule>();
builder.Services.AddScoped<IMatchingRule, SkillMatchingRule>();
builder.Services.AddScoped<IMatchingRule, ExperienceMatchingRule>();
builder.Services.AddScoped<IMatchingRule, CategoryMatchingRule>();
builder.Services.AddScoped<MatchingEngine>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TalentosItContext>();
    
    if (!context.Utilizadors.Any())
    {
        var hasher = new PasswordHasher<Utilizador>();
        var admin = new Utilizador
        {
            PrimeiroNome = "Admin",
            Apelido = "Sistema",
            Email = "admin@talentosit.com",
            TipoUtilizador = TipoUtilizador.Admin,
            Ativo = true,
            Telefone = null
        };
        admin.PalavraPasse = hasher.HashPassword(admin, "Admin123!");
        context.Utilizadors.Add(admin);
        context.SaveChanges();
    }
}

app.Run();
