using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Domain.Common;

namespace Ecommerce.Application.UseCases.Interfaces;

public interface IAuthUseCase
{
    Task<Result<TokenDto>> LoginAsync(LoginDto dto, CancellationToken ct = default);
    Task<Result<TokenDto>> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken ct = default);
}
