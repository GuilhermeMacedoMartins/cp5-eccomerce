using System.Net;
using System.Net.Http.Json;
using Ecommerce.API.Common;

namespace Ecommerce.Tests.Functional;

[Collection("API")]
public class EnderecosControllerTest
{
    private readonly CustomWebApplicationFactory _factory;

    public EnderecosControllerTest(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact(DisplayName = "GET /api/enderecos/{cep} mal formatado devolve 400")]
    [Trait("Funcional", "ServicoExterno")]
    public async Task CepMalFormatado_DeveDevolver400()
    {
        // Arrange
        using var client = await AuthHelper.ComoClienteAsync(_factory);

        // Act
        var resposta = await client.GetAsync("/api/enderecos/123");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Equal("CEP_INVALIDO", corpo?.Erro?.Codigo);
    }

    [Fact(DisplayName = "GET /api/enderecos/{cep} exige autenticacao")]
    [Trait("Funcional", "ServicoExterno")]
    public async Task SemToken_DeveDevolver401()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/enderecos/01001000");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact(DisplayName = "GET /api/enderecos/{cep} valido resolve o endereco")]
    [Trait("Funcional", "ServicoExterno")]
    public async Task CepValido_DeveResolverEndereco()
    {
        // Arrange
        using var client = await AuthHelper.ComoClienteAsync(_factory);

        // Act
        var resposta = await client.GetAsync("/api/enderecos/01001000");

        if (resposta.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            var falha = await resposta.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.Equal("CEP_SERVICO_INDISPONIVEL", falha?.Erro?.Codigo);
            return;
        }

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
        var corpo = await resposta.Content.ReadAsStringAsync();
        Assert.Contains("SP", corpo);
    }
}
