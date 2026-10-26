using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly ApplicationContext _context;
    private readonly ILogger<UsuarioRepository> _logger;

    public UsuarioRepository(ApplicationContext context, ILogger<UsuarioRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<UsuarioEntity>> ObterPorEmailAsync(string email, CancellationToken ct = default)
    {
        var normalizado = email.ToLowerInvariant();
        var usuario = await _context.Usuarios.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Email == normalizado, ct);

        return usuario is null
            ? Result.Failure<UsuarioEntity>(DomainErrors.Autenticacao.CredenciaisInvalidas())
            : Result.Success(usuario);
    }

    public async Task<Result<UsuarioEntity>> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var usuario = await _context.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id, ct);

        return usuario is null
            ? Result.Failure<UsuarioEntity>(Error.NotFound("USUARIO_NAO_ENCONTRADO", $"Usuario {id} nao encontrado."))
            : Result.Success(usuario);
    }

    public async Task<bool> EmailExisteAsync(string email, CancellationToken ct = default)
    {
        var normalizado = email.ToLowerInvariant();
        return await _context.Usuarios.AnyAsync(u => u.Email == normalizado, ct);
    }

    public async Task<Result<UsuarioEntity>> AdicionarAsync(UsuarioEntity usuario, CancellationToken ct = default)
    {
        try
        {
            await _context.Usuarios.AddAsync(usuario, ct);
            await _context.SaveChangesAsync(ct);
            return Result.Success(usuario);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuario");
            return Result.Failure<UsuarioEntity>(
                Error.Unexpected("FALHA_REGISTRO", "Nao foi possivel concluir o registro."));
        }
    }
}
