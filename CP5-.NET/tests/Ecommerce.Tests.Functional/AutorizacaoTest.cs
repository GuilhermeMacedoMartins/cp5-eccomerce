using System.Net;
using System.Net.Http.Json;
using Ecommerce.Application.DTOs.Produto;

namespace Ecommerce.Tests.Functional;

[Collection("API")]
public class AutorizacaoTest
{
    private readonly CustomWebApplicationFactory _factory;

    public AutorizacaoTest(CustomWebApplicationFactory factory) => _factory = factory;

    private static CriarProdutoDto NovoProduto() => new()
    {
        Nome = $"Produto {Guid.NewGuid():N}"[..20],
        Descricao = "Criado no teste funcional",
        Preco = 99.90m,
        QuantidadeEstoque = 5,
        Categoria = "Testes"
    };

    [Fact(DisplayName = "Sem token, POST /api/produtos devolve 401")]
    [Trait("Funcional", "Autorizacao")]
    public async Task SemToken_DeveDevolver401()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var resposta = await client.PostAsJsonAsync("/api/produtos", NovoProduto());

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }

    [Fact(DisplayName = "Com token de Cliente, POST /api/produtos devolve 403")]
    [Trait("Funcional", "Autorizacao")]
    public async Task TokenDeCliente_DeveDevolver403()
    {
        // Arrange
        using var client = await AuthHelper.ComoClienteAsync(_factory);

        // Act
        var resposta = await client.PostAsJsonAsync("/api/produtos", NovoProduto());

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, resposta.StatusCode);
    }

    [Fact(DisplayName = "Com token de Administrador, POST /api/produtos devolve 201")]
    [Trait("Funcional", "Autorizacao")]
    public async Task TokenDeAdministrador_DeveDevolver201()
    {
        // Arrange
        using var client = await AuthHelper.ComoAdminAsync(_factory);

        // Act
        var resposta = await client.PostAsJsonAsync("/api/produtos", NovoProduto());

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);
    }

    [Fact(DisplayName = "GET /api/produtos e publico e dispensa token")]
    [Trait("Funcional", "Autorizacao")]
    public async Task ListagemPublica_DeveDevolver200SemToken()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/produtos");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);
    }

    [Fact(DisplayName = "GET /api/pedidos exige autenticacao")]
    [Trait("Funcional", "Autorizacao")]
    public async Task PedidosSemToken_DeveDevolver401()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/pedidos");

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, resposta.StatusCode);
    }
}
