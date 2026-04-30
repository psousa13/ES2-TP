using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services.Matching
{
    // Regra responsável por verificar se o talento tem todas as skills exigidas pela proposta.
    public class SkillMatchingRule : IMatchingRule
    {
        public bool IsMatch(Talento talento, PropostaTrabalho proposta)
        {
            // Uma proposta sem skills não deve gerar match.
            if (proposta.PropostaSkills == null || !proposta.PropostaSkills.Any())
            {
                return false;
            }

            // Verifica se todas as skills exigidas existem no talento.
            return proposta.PropostaSkills.All(skillExigida =>
                talento.TalentoSkills.Any(skillTalento =>
                    skillTalento.IdSkill == skillExigida.IdSkill
                )
            );
        }
    }
}