using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.UseCases;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Security;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Ecommerce.Tests.Unit.UseCases;

public class AuthUseCaseTest
{
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly Mock<ITokenService> _tokenService = new();
    private readonly IPasswordHasher _hasher = new PasswordHasher();
    private readonly AuthUseCase _useCase;

    public AuthUseCaseTest()
    {
        _useCase = new AuthUseCase(
            _usuarioRepository.Object,
            _tokenService.Object,
            _hasher,
            NullLogger<AuthUseCase>.Instance);

        _tokenService
            .Setup(t => t.GerarToken(It.IsAny<UsuarioEntity>()))
            .Returns(new TokenDto { AccessToken = "token-fake", Perfil = "Cliente" });
    }

    [Fact]
    [Trait("UseCase", "Auth")]
    public async Task Login_DeveAutenticarComSenhaCorreta()
    {
        // Arrange
        var (hash, salt) = _hasher.GerarHash("Senha@123");
        var usuario = new UsuarioEntity
        {
            Id = 1, Nome = "Ana", Email = "ana@teste.com",
            SenhaHash = hash, SenhaSalt = salt, Perfil = PerfilUsuario.Cliente
        };

        _usuarioRepository
            .Setup(r => r.ObterPorEmailAsync("ana@teste.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(usuario));

        // Act
        var resultado = await _useCase.LoginAsync(new LoginDto { Email = "ana@teste.com", Senha = "Senha@123" });

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal("token-fake", resultado.Value.AccessToken);
    }

    [Fact]
    [Trait("UseCase", "Auth")]
    public async Task Login_DeveFalharComSenhaIncorreta()
    {
        // Arrange
        var (hash, salt) = _hasher.GerarHash("Senha@123");
        var usuario = new UsuarioEntity { Id = 1, Email = "ana@teste.com", SenhaHash = hash, SenhaSalt = salt };

        _usuarioRepository
            .Setup(r => r.ObterPorEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(usuario));

        // Act
        var resultado = await _useCase.LoginAsync(new LoginDto { Email = "ana@teste.com", Senha = "errada" });

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("CREDENCIAIS_INVALIDAS", resultado.Error.Code);
        _tokenService.Verify(t => t.GerarToken(It.IsAny<UsuarioEntity>()), Times.Never);
    }

    [Fact]
    [Trait("UseCase", "Auth")]
    public async Task Registrar_DeveRejeitarEmailJaCadastrado()
    {
        // Arrange
        _usuarioRepository
            .Setup(r => r.EmailExisteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var resultado = await _useCase.RegistrarAsync(
            new RegistrarUsuarioDto { Nome = "Ana", Email = "ana@teste.com", Senha = "Senha@123" });

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("EMAIL_JA_CADASTRADO", resultado.Error.Code);
    }
}
