using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services;

public interface IContaService
{
    Task<bool> EmailExisteAsync(string email);
    Task<Utilizador?> ObterUtilizadorPorEmailAsync(string email);
    Task<Utilizador> RegistarUtilizadorAsync(Utilizador utilizador);
    Task CriarClienteAsync(Cliente cliente);
}
