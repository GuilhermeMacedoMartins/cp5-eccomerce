using System.Net;
using System.Net.Http.Json;
using Ecommerce.API.Common;
using Ecommerce.Application.DTOs.Pedido;
using Ecommerce.Application.DTOs.Produto;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Tests.Functional;

[Collection("API")]
public class PedidosControllerTest
{
    private readonly CustomWebApplicationFactory _factory;

    public PedidosControllerTest(CustomWebApplicationFactory factory) => _factory = factory;

    private async Task<int> CriarProdutoAsync(HttpClient admin, int estoque = 10)
    {
        var resposta = await admin.PostAsJsonAsync("/api/produtos", new CriarProdutoDto
        {
            Nome = $"Item {Guid.NewGuid():N}"[..18],
            Descricao = "Produto para teste de pedido",
            Preco = 25.00m,
            QuantidadeEstoque = estoque,
            Categoria = "Testes"
        });

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();
        return corpo?.Dados?.Id ?? 0;
    }

    [Fact(DisplayName = "POST /api/pedidos cria o pedido e preenche o endereco pelo servico externo")]
    [Trait("Funcional", "Pedidos")]
    public async Task Criar_DevePreencherEnderecoViaServicoExterno()
    {
        // Arrange
        using var admin = await AuthHelper.ComoAdminAsync(_factory);
        var produtoId = await CriarProdutoAsync(admin);

        using var cliente = await AuthHelper.ComoClienteAsync(_factory);
        var dto = new CriarPedidoDto
        {
            Cep = "01001000",
            Itens = [new CriarItemPedidoDto { ProdutoId = produtoId, Quantidade = 2 }]
        };

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/pedidos", dto);

        if (resposta.StatusCode == HttpStatusCode.ServiceUnavailable)
        {
            var falha = await resposta.Content.ReadFromJsonAsync<ApiResponse<object>>();
            Assert.Equal("CEP_SERVICO_INDISPONIVEL", falha?.Erro?.Codigo);
            return;
        }

        // Assert
        Assert.Equal(HttpStatusCode.Created, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<PedidoDto>>();
        var pedido = corpo?.Dados;

        Assert.NotNull(pedido);
        Assert.Equal(50.00m, pedido.ValorTotal);
        Assert.Equal(nameof(StatusPedido.Pendente), pedido.Status);
        Assert.Equal("SP", pedido.Endereco.Uf);
        Assert.Contains(pedido.Links, l => l.Rel == "self");
        Assert.Contains(pedido.Links, l => l.Rel == "update-status");
    }

    [Fact(DisplayName = "POST /api/pedidos com CEP invalido devolve 400")]
    [Trait("Funcional", "Pedidos")]
    public async Task Criar_DeveRecusarCepInvalido()
    {
        // Arrange
        using var cliente = await AuthHelper.ComoClienteAsync(_factory);
        var dto = new CriarPedidoDto
        {
            Cep = "123",
            Itens = [new CriarItemPedidoDto { ProdutoId = 1, Quantidade = 1 }]
        };

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/pedidos", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact(DisplayName = "POST /api/pedidos com estoque insuficiente devolve 409")]
    [Trait("Funcional", "Pedidos")]
    public async Task Criar_DeveRecusarEstoqueInsuficiente()
    {
        // Arrange
        using var admin = await AuthHelper.ComoAdminAsync(_factory);
        var produtoId = await CriarProdutoAsync(admin, estoque: 1);

        using var cliente = await AuthHelper.ComoClienteAsync(_factory);
        var dto = new CriarPedidoDto
        {
            Cep = "01001000",
            Itens = [new CriarItemPedidoDto { ProdutoId = produtoId, Quantidade = 500 }]
        };

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/pedidos", dto);

        if (resposta.StatusCode == HttpStatusCode.ServiceUnavailable)
            return; // servico externo fora do ar

        // Assert
        Assert.Equal(HttpStatusCode.Conflict, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Equal("ESTOQUE_INSUFICIENTE", corpo?.Erro?.Codigo);
    }

    [Fact(DisplayName = "POST /api/pedidos com itens duplicados devolve 400")]
    [Trait("Funcional", "Pedidos")]
    public async Task Criar_DeveRecusarItensDuplicados()
    {
        // Arrange
        using var admin = await AuthHelper.ComoAdminAsync(_factory);
        var produtoId = await CriarProdutoAsync(admin);

        using var cliente = await AuthHelper.ComoClienteAsync(_factory);
        var dto = new CriarPedidoDto
        {
            Cep = "01001000",
            Itens =
            [
                new CriarItemPedidoDto { ProdutoId = produtoId, Quantidade = 1 },
                new CriarItemPedidoDto { ProdutoId = produtoId, Quantidade = 2 }
            ]
        };

        // Act
        var resposta = await cliente.PostAsJsonAsync("/api/pedidos", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Equal("ITENS_DUPLICADOS", corpo?.Erro?.Codigo);
    }

    [Fact(DisplayName = "GET /api/pedidos/{id} inexistente devolve 404 (erro lancado pelo repositorio)")]
    [Trait("Funcional", "Pedidos")]
    public async Task ObterPorId_DeveDevolver404()
    {
        // Arrange
        using var cliente = await AuthHelper.ComoClienteAsync(_factory);

        // Act
        var resposta = await cliente.GetAsync("/api/pedidos/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.Equal("PEDIDO_NAO_ENCONTRADO", corpo?.Erro?.Codigo);
    }

    [Fact(DisplayName = "GET /api/pedidos devolve PageResult com links de navegacao")]
    [Trait("Funcional", "Pedidos")]
    public async Task Listagem_DeveDevolverPageResult()
    {
        // Arrange
        using var cliente = await AuthHelper.ComoClienteAsync(_factory);

        // Act
        var resposta = await cliente.GetAsync("/api/pedidos?pageNumber=1&pageSize=5");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<PageResult<PedidoDto>>>();
        Assert.NotNull(corpo?.Dados);
        Assert.Equal(1, corpo.Dados.PageNumber);
        Assert.Contains(corpo.Dados.Links, l => l.Rel == "self");
    }
}
