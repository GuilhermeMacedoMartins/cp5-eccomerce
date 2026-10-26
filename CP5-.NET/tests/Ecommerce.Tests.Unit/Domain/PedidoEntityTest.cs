using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Tests.Unit.Domain;

public class PedidoEntityTest
{
    [Fact]
    [Trait("Dominio", "Pedido")]
    public void RecalcularValorTotal_DeveSomarOsSubtotais()
    {
        // Arrange
        var pedido = new PedidoEntity { ClienteNome = "Ana", ClienteEmail = "ana@teste.com" };
        pedido.Itens.Add(new ItemPedidoEntity { ProdutoId = 1, Quantidade = 2, PrecoUnitario = 50m });
        pedido.Itens.Add(new ItemPedidoEntity { ProdutoId = 2, Quantidade = 3, PrecoUnitario = 10m });

        // Act
        pedido.RecalcularValorTotal();

        // Assert
        Assert.Equal(130m, pedido.ValorTotal);
    }

    [Theory]
    [Trait("Dominio", "Pedido")]
    [InlineData(StatusPedido.Entregue)]
    [InlineData(StatusPedido.Cancelado)]
    public void AlterarStatus_DeveBloquearPedidoFinalizado(StatusPedido statusFinal)
    {
        // Arrange
        var pedido = new PedidoEntity { Id = 7, Status = statusFinal };

        // Act
        var ex = Assert.Throws<RegraDeNegocioException>(() => pedido.AlterarStatus(StatusPedido.Enviado));

        // Assert
        Assert.Equal("STATUS_FINALIZADO", ex.Error.Code);
        Assert.Equal(statusFinal, pedido.Status);
    }

    [Fact]
    [Trait("Dominio", "Pedido")]
    public void AlterarStatus_DevePermitirTransicaoValida()
    {
        // Arrange
        var pedido = new PedidoEntity { Id = 1, Status = StatusPedido.Pendente };

        // Act
        pedido.AlterarStatus(StatusPedido.Confirmado);

        // Assert
        Assert.Equal(StatusPedido.Confirmado, pedido.Status);
    }

    [Fact]
    [Trait("Dominio", "Pedido")]
    public void DefinirEndereco_DevePreencherOsCamposDoServicoExterno()
    {
        // Arrange
        var pedido = new PedidoEntity();

        // Act
        pedido.DefinirEndereco("01001-000", "Praca da Se", "Se", "Sao Paulo", "SP");

        // Assert
        Assert.Equal("01001-000", pedido.Cep);
        Assert.Equal("Sao Paulo", pedido.Cidade);
        Assert.Equal("SP", pedido.Uf);
    }
}
