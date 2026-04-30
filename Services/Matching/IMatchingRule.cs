using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services.Matching
{
    // Interface comum para todas as regras de matching.
    // Cada nova regra deve implementar esta interface.
    public interface IMatchingRule
    {
        bool IsMatch(Talento talento, PropostaTrabalho proposta);
    }
}