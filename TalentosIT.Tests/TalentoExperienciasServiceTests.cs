using NUnit.Framework;
using Microsoft.EntityFrameworkCore;
using static NUnit.Framework.Assert;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;

namespace TalentosIT.Tests;

[TestFixture]
public class TalentoExperienciasServiceTests
{
    private DbContextOptions<TalentosItContext> _options;
    private TalentosItContext _context;
    private TalentoExperienciasService _service;

    [SetUp]
    public void Setup()
    {
        _options = new DbContextOptionsBuilder<TalentosItContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new TalentosItContext(_options);
        _service = new TalentoExperienciasService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public void GetIdNulo()
    {
        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetTalentoComExperiencias(null));
    }

    [Test]
    public void GetTalentoInexistente()
    {
        Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetTalentoComExperiencias(1));
    }

    [Test]
    public async Task GetTalentoValido()
    {
        // Arrange
        var talento = CriarTalento();
        _context.Talentos.Add(talento);
        await _context.SaveChangesAsync();
        // Act
        var resultado = await _service.GetTalentoComExperiencias(1);
        // Assert
        Assert.That(resultado, Is.Not.Null);
        Assert.That(resultado.IdTalento, Is.EqualTo(1));
    }

    [Test]
    public async Task CriarExperienciaValida()
    {
        // Arrange
        var talento = CriarTalento();
        _context.Talentos.Add(talento);
        await _context.SaveChangesAsync();

        var experiencia = new Experiencia
        {
            IdExperiencia = 10, IdTalento = 1,
            AnoInicio = 2022, AnoFim = 2024,
            Titulo = "Designer", Empresa = "aaa"
        };
        // Act
        await _service.Criar(experiencia);
        // Assert
        var experienciaRegistada = await _context.Experiencias.FindAsync(10);
        Assert.That(experienciaRegistada, Is.Not.Null);
        Assert.That(experienciaRegistada.Titulo, Is.EqualTo("Designer"));
    }

    [Test]
    public async Task CriarExperienciaPeriodoImpossivel()
    {
        var talento = CriarTalento();
        _context.Talentos.Add(talento);
        await _context.SaveChangesAsync();

        var experiencia = new Experiencia
        {
            IdTalento = 1,
            AnoInicio = 2024, AnoFim = 2022,
            Titulo = "Estagiário"
        };

        var e = Assert.ThrowsAsync<BusinessException>(async () => await _service.Criar(experiencia));
        Assert.That(e.Message, Is.EqualTo("O ano de fim deve ser igual ou superior ao ano de início."));
    }

    [Test]
    public async Task CriarExperienciaAnoFuturo()
    {
        var talento = CriarTalento();
        _context.Talentos.Add(talento);
        await _context.SaveChangesAsync();

        var experiencia = new Experiencia
        {
            IdTalento = 1,
            AnoInicio = 2030, AnoFim = 2031,
            Titulo = "Desenvolvedor"
        };

        var e = Assert.ThrowsAsync<BusinessException>(async () => await _service.Criar(experiencia));
        Assert.That(e.Message, Is.EqualTo("O ano de início não pode ser no futuro."));
    }

    [Test]
    public async Task CriarExperienciasSobrepostas()
    {
        var talento = CriarTalento();
        _context.Talentos.Add(talento);
        await _context.SaveChangesAsync();

        var experiencia1 = new Experiencia {
            IdExperiencia = 1, IdTalento = 1,
            AnoInicio = 2020, AnoFim = 2023,
            Titulo = "Designer", Empresa = "aaa"
        };
        await _service.Criar(experiencia1);

        var experiencia2 = new Experiencia {
            IdExperiencia = 2, IdTalento = 1,
            AnoInicio = 2022, AnoFim = 2025,
            Titulo = "Developer", Empresa = "bbb"
        };
        var e = Assert.ThrowsAsync<BusinessException>(async () => await _service.Criar(experiencia2));
        Assert.That(e.Message, Does.Contain("O período sobrepõe-se com a experiência"));
    }

    [Test]
    public async Task CriarExperienciasSequenciais()
    {
        // Arrange
        var talento = CriarTalento();
        _context.Talentos.Add(talento);
        await _context.SaveChangesAsync();

        var experiencia1 = new Experiencia {
            IdExperiencia = 1, IdTalento = 1,
            AnoInicio = 2020, AnoFim = 2023,
            Titulo = "Designer", Empresa = "aaa"
        };
        var experiencia2 = new Experiencia
        {
            IdExperiencia = 2, IdTalento = 1,
            AnoInicio = 2023, AnoFim = 2025,
            Titulo = "Developer", Empresa = "bbb"
        };
        // Act
        await _service.Criar(experiencia1);
        await _service.Criar(experiencia2);
        // Assert
        var experienciaRegistada = await _context.Experiencias.FindAsync(2);
        Assert.That(experienciaRegistada, Is.Not.Null);
        Assert.That(experienciaRegistada.Titulo, Is.EqualTo("Developer"));
    }

    [Test]
    public void EditarIdsDiferentes()
    {
        var model = new Experiencia { IdExperiencia = 5 };
        Assert.ThrowsAsync<NotFoundException>(async () => await _service.Editar(99, model));
    }

    [Test]
    public async Task EditarValido()
    {
        // Arrange
        var talento = CriarTalento();
        _context.Talentos.Add(talento);
        await _context.SaveChangesAsync();

        var experiencia = new Experiencia {
            IdExperiencia = 1, IdTalento = 1,
            AnoInicio = 2020, AnoFim = 2021,
            Titulo = "Developer", Empresa = "aaa"
        };
        _context.Experiencias.Add(experiencia);
        await _context.SaveChangesAsync();
        _context.Entry(experiencia).State = EntityState.Detached;

        var experienciaAtualizada = new Experiencia {
            IdExperiencia = 1, IdTalento = 1,
            AnoInicio = 2020, AnoFim = 2022,
            Titulo = "Developer", Empresa = "bbb"
        };
        // Act
        await _service.Editar(1, experienciaAtualizada);
        // Assert
        var resultado = await _context.Experiencias.FindAsync(1);
        Assert.That(resultado.Empresa, Is.EqualTo("bbb"));
        Assert.That(resultado.AnoFim, Is.EqualTo(2022));
    }

    [Test]
    public void EliminarExperienciaInexistente()
    {
        Assert.ThrowsAsync<NotFoundException>(async () => await _service.Eliminar(999));
    }

    [Test]
    public async Task EliminarExperienciaValida()
    {
        // Arrange
        var experiencia = new Experiencia
        {
            IdExperiencia = 2, IdTalento = 1,
            AnoInicio = 2020, AnoFim = 2023,
            Titulo = "Designer", Empresa = "ccc"
        };
        _context.Experiencias.Add(experiencia);
        await _context.SaveChangesAsync();
        // Act
        await _service.Eliminar(2);
        // Assert
        var experienciaRegistada = await _context.Experiencias.FindAsync(5);
        Assert.That(experienciaRegistada, Is.Null);
    }

    private Talento CriarTalento()
    {
        return new Talento
        {
            IdTalento = 1,
            PrimeiroNome = "Maria", Apelido = "Maria",
            Email = "mariamaria@gmail.com", Pais = "Portugal",
            PrecoHora = 10.00, Categoria = "Developer"
        };
    }
}
