using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services.Matching
{
    // Motor central de matching.
    // Está fechado à modificação, porque não precisa de ser alterado
    // quando uma nova regra de matching é adicionada.
    public class MatchingEngine
    {
        private readonly IEnumerable<IMatchingRule> _rules;

        public MatchingEngine(IEnumerable<IMatchingRule> rules)
        {
            _rules = rules;
        }

        public bool IsMatch(Talento talento, PropostaTrabalho proposta)
        {
            // O talento só faz match se cumprir todas as regras registadas.
            return _rules.All(rule => rule.IsMatch(talento, proposta));
        }
    }
}