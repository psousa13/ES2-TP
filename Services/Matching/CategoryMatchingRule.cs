using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services.Matching
{
    // Regra responsável por verificar se a categoria do talento é compatível com a categoria da proposta.
    public class CategoryMatchingRule : IMatchingRule
    {
        public bool IsMatch(Talento talento, PropostaTrabalho proposta)
        {
            // Se uma das categorias não estiver preenchida, a regra não bloqueia o matching.
            // Isto evita esconder resultados por falta de dados.
            if (string.IsNullOrWhiteSpace(talento.Categoria) ||
                string.IsNullOrWhiteSpace(proposta.Categoria))
            {
                return true;
            }

            return talento.Categoria.Trim().ToLower() == proposta.Categoria.Trim().ToLower();
        }
    }
}