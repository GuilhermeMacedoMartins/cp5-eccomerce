using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Tests.Unit.Domain;

public class ProdutoEntityTest
{
    [Theory]
    [Trait("Dominio", "Produto")]
    [InlineData(10, 3, 7)]
    [InlineData(10, 10, 0)]
    public void BaixarEstoque_DeveSubtrairCorretamente(int estoque, int baixa, int esperado)
    {
        // Arrange
        var produto = new ProdutoEntity { Nome = "Mouse", QuantidadeEstoque = estoque, Ativo = true };

        // Act
        produto.BaixarEstoque(baixa);

        // Assert
        Assert.Equal(esperado, produto.QuantidadeEstoque);
    }

    [Fact]
    [Trait("Dominio", "Produto")]
    public void BaixarEstoque_DeveLancarErroCustomizado_QuandoEstoqueInsuficiente()
    {
        // Arrange
        var produto = new ProdutoEntity { Nome = "Teclado", QuantidadeEstoque = 2, Ativo = true };

        // Act
        var ex = Assert.Throws<RegraDeNegocioException>(() => produto.BaixarEstoque(5));

        // Assert
        Assert.Equal("ESTOQUE_INSUFICIENTE", ex.Error.Code);
        Assert.Equal(2, produto.QuantidadeEstoque);
    }

    [Fact]
    [Trait("Dominio", "Produto")]
    public void BaixarEstoque_DeveLancar_QuandoProdutoInativo()
    {
        // Arrange
        var produto = new ProdutoEntity { Nome = "Monitor", QuantidadeEstoque = 10, Ativo = false };

        // Act
        var ex = Assert.Throws<RegraDeNegocioException>(() => produto.BaixarEstoque(1));

        // Assert
        Assert.Equal("PRODUTO_INATIVO", ex.Error.Code);
    }

    [Fact]
    [Trait("Dominio", "Produto")]
    public void ReporEstoque_DeveSomarAoEstoque()
    {
        // Arrange
        var produto = new ProdutoEntity { Nome = "Webcam", QuantidadeEstoque = 5 };

        // Act
        produto.ReporEstoque(3);

        // Assert
        Assert.Equal(8, produto.QuantidadeEstoque);
    }
}
