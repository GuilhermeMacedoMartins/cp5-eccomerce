using System.Net;
using System.Net.Http.Json;
using Ecommerce.API.Common;
using Ecommerce.Application.DTOs.Produto;
using Ecommerce.Domain.Common;

namespace Ecommerce.Tests.Functional;

[Collection("API")]
public class ProdutosControllerTest
{
    private readonly CustomWebApplicationFactory _factory;

    public ProdutosControllerTest(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact(DisplayName = "GET /api/produtos devolve PageResult com metadados e links")]
    [Trait("Funcional", "Produtos")]
    public async Task Listagem_DeveDevolverPageResultComHateoas()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/produtos?pageNumber=1&pageSize=2");

        // Assert
        Assert.Equal(HttpStatusCode.OK, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<PageResult<ProdutoDto>>>();
        var pagina = corpo?.Dados;

        Assert.NotNull(pagina);
        Assert.Equal(1, pagina.PageNumber);
        Assert.Equal(2, pagina.PageSize);
        Assert.True(pagina.TotalRecords >= 5);
        Assert.True(pagina.TotalPages >= 3);
        Assert.True(pagina.HasNext);
        Assert.False(pagina.HasPrevious);

        Assert.Contains(pagina.Links, l => l.Rel == "self");
        Assert.Contains(pagina.Links, l => l.Rel == "next");
        Assert.Contains(pagina.Links, l => l.Rel == "last");

        Assert.All(pagina.Data, p => Assert.Contains(p.Links, l => l.Rel == "self" && l.Method == "GET"));
    }

    [Fact(DisplayName = "GET /api/produtos limita o pageSize a 50")]
    [Trait("Funcional", "Produtos")]
    public async Task Listagem_DeveLimitarOPageSize()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/produtos?pageNumber=1&pageSize=9999");

        // Assert
        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<PageResult<ProdutoDto>>>();
        Assert.Equal(50, corpo?.Dados?.PageSize);
    }

    [Fact(DisplayName = "GET /api/produtos/{id} inexistente devolve 404 com erro customizado")]
    [Trait("Funcional", "Produtos")]
    public async Task ObterPorId_DeveDevolver404ComCodigoDoErro()
    {
        // Arrange
        using var client = _factory.CreateClient();

        // Act
        var resposta = await client.GetAsync("/api/produtos/999999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, resposta.StatusCode);

        var corpo = await resposta.Content.ReadFromJsonAsync<ApiResponse<object>>();
        Assert.False(corpo?.Sucesso);
        Assert.Equal("PRODUTO_NAO_ENCONTRADO", corpo?.Erro?.Codigo);
        Assert.Equal("NotFound", corpo?.Erro?.Tipo);
    }

    [Fact(DisplayName = "POST /api/produtos com preco zero devolve 400")]
    [Trait("Funcional", "Produtos")]
    public async Task Criar_DeveRecusarPrecoZero()
    {
        // Arrange
        using var client = await AuthHelper.ComoAdminAsync(_factory);
        var dto = new CriarProdutoDto { Nome = "Invalido", Preco = 0m, QuantidadeEstoque = 1, Categoria = "Testes" };

        // Act
        var resposta = await client.PostAsJsonAsync("/api/produtos", dto);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, resposta.StatusCode);
    }

    [Fact(DisplayName = "Fluxo completo: cria, consulta e atualiza um produto")]
    [Trait("Funcional", "Produtos")]
    public async Task FluxoCompleto_DeveFuncionar()
    {
        // Arrange
        using var client = await AuthHelper.ComoAdminAsync(_factory);
        var novo = new CriarProdutoDto
        {
            Nome = "Mousepad XL",
            Descricao = "900x400mm",
            Preco = 89.90m,
            QuantidadeEstoque = 20,
            Categoria = "Acessorios"
        };

        // Act
        var criacao = await client.PostAsJsonAsync("/api/produtos", novo);
        var criado = await criacao.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();
        var id = criado?.Dados?.Id ?? 0;

        // Assert
        Assert.Equal(HttpStatusCode.Created, criacao.StatusCode);
        Assert.True(id > 0);
        Assert.Contains(criado!.Dados!.Links, l => l.Rel == "update");

        // Act
        var consulta = await client.GetAsync($"/api/produtos/{id}");
        var lido = await consulta.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        Assert.Equal("Mousepad XL", lido?.Dados?.Nome);

        // Act
        var atualizacao = await client.PutAsJsonAsync($"/api/produtos/{id}", new AtualizarProdutoDto
        {
            Nome = "Mousepad XL Pro",
            Descricao = novo.Descricao,
            Preco = 109.90m,
            QuantidadeEstoque = 15,
            Categoria = novo.Categoria,
            Ativo = true
        });

        var atualizado = await atualizacao.Content.ReadFromJsonAsync<ApiResponse<ProdutoDto>>();

        // Assert
        Assert.Equal(HttpStatusCode.OK, atualizacao.StatusCode);
        Assert.Equal("Mousepad XL Pro", atualizado?.Dados?.Nome);
        Assert.Equal(109.90m, atualizado?.Dados?.Preco);
    }
}
