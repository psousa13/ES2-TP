using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TalentosIT.Web.Exceptions;
using TalentosIT.Web.Models;
using TalentosIT.Web.Services;

namespace TalentosIT.Tests.Services
{
    [TestFixture]
    public class TalentoSkillsServiceTests
    {
        private DbContextOptions<TalentosItContext> _options;
        private TalentosItContext _context;
        private Mock<RegistoAtividadeService> _mockRegistoService;
        private TalentoSkillsService _service;

        [SetUp]
        public void Setup()
        {
            _options = new DbContextOptionsBuilder<TalentosItContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new TalentosItContext(_options);
            _mockRegistoService = new Mock<RegistoAtividadeService>(_context);
            _service = new TalentoSkillsService(_context, _mockRegistoService.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task GetSkillsDisponiveis()
        {
            // Arrange
            var talento = new Talento
            {
                IdTalento = 1,
                PrimeiroNome = "Maria", Apelido = "Maria",
                Email = "mariamaria@gmail.com", Pais = "Portugal",
                PrecoHora = 10.00, Categoria = "Developer"
            };
            var skill1 = new Skill { IdSkill = 1, Nome = "C#" };
            var skill2 = new Skill { IdSkill = 2, Nome = "Java" };
            var skill3 = new Skill { IdSkill = 3, Nome = "SQL" };

            _context.Talentos.Add(talento);
            _context.Skills.AddRange(skill1, skill2, skill3);

            var talentoSkill = new TalentoSkill { IdTalento = 1, IdSkill = 1 };
            _context.TalentoSkills.Add(talentoSkill);
            await _context.SaveChangesAsync();

            // Act
            var (talentoRegistado, skillsDisponiveis) = await _service.GetDadosGestao(1);

            // Assert
            Assert.That(talentoRegistado, Is.Not.Null);
            Assert.That(talentoRegistado.IdTalento, Is.EqualTo(1));

            Assert.That(skillsDisponiveis.Count, Is.EqualTo(2));
            Assert.That(skillsDisponiveis, Has.None.Matches<Skill>(s => s.IdSkill == 1));
        }

        [Test]
        public void GetIdNulo()
        {
            Assert.ThrowsAsync<NotFoundException>(async () => await _service.GetDadosGestao(null));
        }

        [Test]
        public void CriarSkillAnosNegativos()
        {
            var e = Assert.ThrowsAsync<BusinessException>(async () => await _service.AdicionarSkill(1, 10, -2, null));
            Assert.That(e.Message, Is.EqualTo("Os anos de experiência não podem ser negativos."));
        }

        [Test]
        public async Task CriarTalentoSkillExistente()
        {
            var ts = new TalentoSkill { IdTalento = 1, IdSkill = 10, AnosExperiencia = 3 };
            _context.TalentoSkills.Add(ts);
            await _context.SaveChangesAsync();

            Assert.ThrowsAsync<AlreadyRegisteredException>(async () => await _service.AdicionarSkill(1, 10, 5, null));
        }

        [Test]
        public async Task EditarSkillInexistente()
        {
            Assert.ThrowsAsync<NotFoundException>(async () => await _service.EditarSkill(1, 999, 5));
        }

        [Test]
        public async Task EditSkillValida()
        {
            // Arrange
            var ts = new TalentoSkill { IdTalento = 1, IdSkill = 10, AnosExperiencia = 2 };
            _context.TalentoSkills.Add(ts);
            await _context.SaveChangesAsync();
            _context.Entry(ts).State = EntityState.Detached;

            // Act
            await _service.EditarSkill(1, 10, 6);

            // Assert
            var skillAtualizada = await _context.TalentoSkills.FirstOrDefaultAsync(ts => ts.IdTalento == 1 && ts.IdSkill == 10);
            Assert.That(skillAtualizada.AnosExperiencia, Is.EqualTo(6));
        }

        [Test]
        public async Task RemoverSkill()
        {
            // Arrange
            var ts = new TalentoSkill { IdTalento = 2, IdSkill = 20, AnosExperiencia = 1 };
            _context.TalentoSkills.Add(ts);
            await _context.SaveChangesAsync();

            // Act
            await _service.RemoverSkill(2, 20, 1);

            // Assert
            var existe = await _context.TalentoSkills.AnyAsync(ts => ts.IdTalento == 2 && ts.IdSkill == 20);
            Assert.That(existe, Is.False);
        }
    }
}