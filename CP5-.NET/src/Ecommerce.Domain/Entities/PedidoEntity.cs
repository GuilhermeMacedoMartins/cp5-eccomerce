using Ecommerce.Domain.Common;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class PedidoEntity
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }
    public UsuarioEntity? Usuario { get; set; }

    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; } = DateTime.UtcNow;
    public StatusPedido Status { get; set; } = StatusPedido.Pendente;
    public decimal ValorTotal { get; set; }

    public string Cep { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;

    public ICollection<ItemPedidoEntity> Itens { get; set; } = new List<ItemPedidoEntity>();

    public void RecalcularValorTotal() => ValorTotal = Itens.Sum(i => i.Subtotal);

    public void AlterarStatus(StatusPedido novoStatus)
    {
        if (Status is StatusPedido.Entregue or StatusPedido.Cancelado)
            throw new RegraDeNegocioException(DomainErrors.Pedido.StatusFinalizado(Id, Status.ToString()));

        Status = novoStatus;
    }

    public void DefinirEndereco(string cep, string logradouro, string bairro, string cidade, string uf)
    {
        Cep = cep;
        Logradouro = logradouro;
        Bairro = bairro;
        Cidade = cidade;
        Uf = uf;
    }
}
