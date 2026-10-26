using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IUsuarioRepository
{
    Task<Result<UsuarioEntity>> ObterPorEmailAsync(string email, CancellationToken ct = default);
    Task<Result<UsuarioEntity>> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<bool> EmailExisteAsync(string email, CancellationToken ct = default);
    Task<Result<UsuarioEntity>> AdicionarAsync(UsuarioEntity usuario, CancellationToken ct = default);
}
