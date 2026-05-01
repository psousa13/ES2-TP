using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services.Matching
{
    // Regra responsável por verificar se o talento tem os anos mínimos de experiência exigidos.
    public class ExperienceMatchingRule : IMatchingRule
    {
        public bool IsMatch(Talento talento, PropostaTrabalho proposta)
        {
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