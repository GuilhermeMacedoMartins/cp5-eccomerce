using System.Net;
using System.Net.Http.Json;
using Ecommerce.API.Common;
using Ecommerce.Application.DTOs.Auth;

namespace Ecommerce.Tests.Functional;

[Collection("API")]
public class AuthControllerTest
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTest(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact(DisplayName = "POST /api/auth/login devolve token para credenciais validas")]
    [Trait("Funcional", "Auth")]
    public async Task Login_DeveDevolverToken()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var dto = new LoginDto { Email = AuthHelper.EmailAdmin, Senha = AuthHelper.SenhaAdmin };

        // Act
        var resposta = await client.PostAsJsonAsync("/api/auth/login", dto);

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<TokenDto>>();
        Assert.NotNull(corpo);
        Assert.True(corpo.Sucesso);
        Assert.False(string.IsNullOrWhiteSpace(corpo.Dados?.AccessToken));
        Assert.Equal("Administrador", corpo.Dados?.Perfil);
    }

    [Fact(DisplayName = "POST /api/auth/login recusa senha incorreta")]
    [Trait("Funcional", "Auth")]
    public async Task Login_DeveRecusarSenhaIncorreta()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var dto = new LoginDto { Email = AuthHelper.EmailAdmin, Senha = "senha-errada" };

        // Act
        var resposta = await client.PostAsJsonAsync("/api/auth/login", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.False(corpo?.Sucesso);
        Assert.Equal("CREDENCIAIS_INVALIDAS", corpo?.Erro?.Codigo);
    }

    [Fact(DisplayName = "POST /api/auth/registrar cria usuario e devolve token")]
    [Trait("Funcional", "Auth")]
    public async Task Registrar_DeveCriarUsuario()
    {
        // Arrange
        using var client = _factory.CreateClient();
        var dto = new RegistrarUsuarioDto
        {
            Nome = "Novo Usuario",
            Email = $"novo-{Guid.NewGuid():N}@teste.com",
            Senha = "Senha@123"
        };

        // Act
        var resposta = await client.PostAsJsonAsync("/api/auth/registrar", dto);

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<TokenDto>>();
        Assert.Equal("Cliente", corpo?.Dados?.Perfil);
    }
}
