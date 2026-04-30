namespace TalentosIT.Web.DTO;

public class EditClienteDTO
{
    public int IdCliente { get; set; }

    public int IdUtilizador { get; set; }

    public string PrimeiroNome { get; set; } = null!;

    public string Apelido { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Telefone { get; set; }

    public string Rua { get; set; } = null!;

    public string NumPorta { get; set; } = null!;

    public string Cidade { get; set; } = null!;

    public string Pais { get; set; } = null!;
}