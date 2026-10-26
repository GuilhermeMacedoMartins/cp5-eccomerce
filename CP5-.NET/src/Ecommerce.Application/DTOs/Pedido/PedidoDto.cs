using Ecommerce.Domain.Common;

namespace Ecommerce.Application.DTOs.Pedido;

public class PedidoDto : IHateoasResource
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }

    public EnderecoDto Endereco { get; set; } = new();
    public List<ItemPedidoDto> Itens { get; set; } = new();

    public List<Link> Links { get; set; } = new();
}
