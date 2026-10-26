using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Interfaces;

public interface ITokenService
{
    TokenDto GerarToken(UsuarioEntity usuario);
}

public interface IPasswordHasher
{
    (string hash, string salt) GerarHash(string senha);
    bool Verificar(string senha, string hash, string salt);
}
