using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.UseCases.Interfaces;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.UseCases;

public class AuthUseCase : IAuthUseCase
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly ITokenService _tokenService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<AuthUseCase> _logger;

    public AuthUseCase(
        IUsuarioRepository usuarioRepository,
        ITokenService tokenService,
        IPasswordHasher passwordHasher,
        ILogger<AuthUseCase> logger)
    {
        _usuarioRepository = usuarioRepository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
        _logger = logger;
    }

    public async Task<Result<TokenDto>> LoginAsync(LoginDto dto, CancellationToken ct = default)
    {
        var busca = await _usuarioRepository.ObterPorEmailAsync(dto.Email, ct);

        if (busca.IsFailure)
        {
            _logger.LogWarning("Tentativa de login com e-mail inexistente");
            return Result.Failure<TokenDto>(DomainErrors.Autenticacao.CredenciaisInvalidas());
        }

        var usuario = busca.Value;

        if (!_passwordHasher.Verificar(dto.Senha, usuario.SenhaHash, usuario.SenhaSalt))
        {
            _logger.LogWarning("Senha incorreta para o usuario {UsuarioId}", usuario.Id);
            return Result.Failure<TokenDto>(DomainErrors.Autenticacao.CredenciaisInvalidas());
        }

        _logger.LogInformation("Usuario {UsuarioId} autenticado", usuario.Id);
        return Result.Success(_tokenService.GerarToken(usuario));
    }

    public async Task<Result<TokenDto>> RegistrarAsync(RegistrarUsuarioDto dto, CancellationToken ct = default)
    {
        if (await _usuarioRepository.EmailExisteAsync(dto.Email, ct))
            return Result.Failure<TokenDto>(DomainErrors.Autenticacao.EmailJaCadastrado(dto.Email));

        var (hash, salt) = _passwordHasher.GerarHash(dto.Senha);

        var usuario = new UsuarioEntity
        {
            Nome = dto.Nome,
            Email = dto.Email.ToLowerInvariant(),
            SenhaHash = hash,
            SenhaSalt = salt,
            Perfil = PerfilUsuario.Cliente,
            DataCriacao = DateTime.UtcNow
        };

        var criacao = await _usuarioRepository.AdicionarAsync(usuario, ct);

        if (criacao.IsFailure)
            return Result.Failure<TokenDto>(criacao.Error);

        _logger.LogInformation("Usuario {UsuarioId} registrado", criacao.Value.Id);
        return Result.Success(_tokenService.GerarToken(criacao.Value));
    }
}
