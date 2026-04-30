namespace TalentosIT.Web.DTO;

public class CreateTalentoDTO
{
    public int IdTalento { get; set; }

    public int IdUtilizador { get; set; }

    public double? PrecoHora { get; set; }

    public bool Publico { get; set; }

    public string? Categoria { get; set; }
}