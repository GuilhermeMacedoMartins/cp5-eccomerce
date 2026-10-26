using Ecommerce.Application.DTOs.Pedido;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Application.Mappings;

public static class PedidoMapper
{
    public static ItemPedidoDto ToDto(this ItemPedidoEntity e) => new()
    {
        Id = e.Id,
        ProdutoId = e.ProdutoId,
        ProdutoNome = e.Produto?.Nome ?? string.Empty,
        Quantidade = e.Quantidade,
        PrecoUnitario = e.PrecoUnitario,
        Subtotal = e.Subtotal
    };

    public static PedidoDto ToDto(this PedidoEntity e) => new()
    {
        Id = e.Id,
        UsuarioId = e.UsuarioId,
        ClienteNome = e.ClienteNome,
        ClienteEmail = e.ClienteEmail,
        DataPedido = e.DataPedido,
        Status = e.Status.ToString(),
        ValorTotal = e.ValorTotal,
        Endereco = new EnderecoDto
        {
            Cep = e.Cep,
            Logradouro = e.Logradouro,
            Bairro = e.Bairro,
            Cidade = e.Cidade,
            Uf = e.Uf
        },
        Itens = e.Itens.Select(ToDto).ToList()
    };

    public static List<PedidoDto> ToDtoList(this IEnumerable<PedidoEntity> entities) =>
        entities.Select(ToDto).ToList();
}
