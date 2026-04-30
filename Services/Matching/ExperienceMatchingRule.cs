using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services.Matching
{
    // Regra responsável por verificar se o talento tem os anos mínimos de experiência exigidos.
    public class ExperienceMatchingRule : IMatchingRule
    {
        public bool IsMatch(Talento talento, PropostaTrabalho proposta)
        {
            // Uma proposta sem skills não deve gerar match.
            if (proposta.PropostaSkills == null || !proposta.PropostaSkills.Any())
            {
                return false;
            }

            // Para cada skill exigida, verifica se o talento tem experiência suficiente.
            return proposta.PropostaSkills.All(skillExigida =>
            {
                var skillTalento = talento.TalentoSkills
                    .FirstOrDefault(ts => ts.IdSkill == skillExigida.IdSkill);

                return skillTalento != null &&
                       skillTalento.AnosExperiencia >= skillExigida.AnosMinimosExperiencia;
            });
        }
    }
}