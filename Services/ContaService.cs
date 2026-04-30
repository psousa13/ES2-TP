using Microsoft.EntityFrameworkCore;
using TalentosIT.Web.Models;

namespace TalentosIT.Web.Services;

public class ContaService : IContaService
{
    private readonly TalentosItContext _context;

    public ContaService(TalentosItContext context)
    {
        _context = context;
    }

    public async Task<bool> EmailExisteAsync(string email)
    {
        return await _context.Utilizadors.AnyAsync(u => u.Email == email);
    }

    public async Task<Utilizador?> ObterUtilizadorPorEmailAsync(string email)
    {
        return await _context.Utilizadors.FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<Utilizador> RegistarUtilizadorAsync(Utilizador utilizador)
    {
        _context.Utilizadors.Add(utilizador);
        await _context.SaveChangesAsync();
        return utilizador;
    }

    public async Task CriarClienteAsync(Cliente cliente)
    {
        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();
    }
}
