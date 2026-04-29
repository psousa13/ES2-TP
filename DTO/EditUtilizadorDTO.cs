using TalentosIT.Web.Models;

namespace TalentosIT.Web.DTO;

public class EditUtilizadorDTO
{
    public int IdUtilizador { get; set; }

    public string PrimeiroNome { get; set; } = null!;

    public string Apelido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefone { get; set; }

    public string PalavraPasse { get; set; } = null!;

    public TipoUtilizador TipoUtilizador { get; set; }

    public bool Ativo { get; set; }
}