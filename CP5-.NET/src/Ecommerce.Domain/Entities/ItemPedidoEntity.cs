namespace Ecommerce.Domain.Entities;

public class ItemPedidoEntity
{
    public int Id { get; set; }

    public int PedidoId { get; set; }
    public PedidoEntity? Pedido { get; set; }

    public int ProdutoId { get; set; }
    public ProdutoEntity? Produto { get; set; }

    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }

    public decimal Subtotal => Quantidade * PrecoUnitario;
}
