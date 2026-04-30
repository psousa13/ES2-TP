namespace TalentosIT.Web.DTO;

public class EditPropostaDTO
{
    public int IdProposta { get; set; }

    public int IdUtilizador { get; set; }

    public int IdCliente { get; set; }

    public string Titulo { get; set; } = null!;

    public string? Categoria { get; set; }

    public int? HorasTotais { get; set; }

    public string? Descricao { get; set; }
}