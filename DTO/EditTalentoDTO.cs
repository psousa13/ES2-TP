namespace TalentosIT.Web.DTO;

public class EditTalentoDTO
{
    public int IdTalento { get; set; }

    public int IdUtilizador { get; set; }

    public string? Telefone { get; set; }

    public double? PrecoHora { get; set; }

    public bool Publico { get; set; }
}