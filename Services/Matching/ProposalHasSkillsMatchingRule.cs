using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services.Matching
{
    // Regra que garante que a proposta tem pelo menos uma skill definida.
    // Sem skills exigidas, não faz sentido calcular matching.
    public class ProposalHasSkillsMatchingRule : IMatchingRule
    {
        public bool IsMatch(Talento talento, PropostaTrabalho proposta)
        {
            return proposta.PropostaSkills != null && proposta.PropostaSkills.Any();
        }
    }
}