using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services.Matching
{
    // Regra responsável por verificar se o talento tem todas as skills exigidas pela proposta.
    public class SkillMatchingRule : IMatchingRule
    {
        public bool IsMatch(Talento talento, PropostaTrabalho proposta)
        {
            // Esta regra só verifica se as skills existem no talento.
            // A validação de "proposta tem skills" fica numa regra separada.
            return proposta.PropostaSkills.All(skillExigida =>
                talento.TalentoSkills.Any(skillTalento =>
                    skillTalento.IdSkill == skillExigida.IdSkill
                )
            );
        }
    }
}