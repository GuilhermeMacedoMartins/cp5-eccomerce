using Ecommerce.Domain.Common;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.Domain.Entities;

public class ProdutoEntity
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Ativo { get; set; } = true;
    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public ICollection<ItemPedidoEntity> ItensPedido { get; set; } = new List<ItemPedidoEntity>();

    public void BaixarEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraDeNegocioException(
                Error.Validation("QUANTIDADE_INVALIDA", "A quantidade deve ser maior que zero."));

        if (!Ativo)
            throw new RegraDeNegocioException(DomainErrors.Produto.Inativo(Nome));

        if (quantidade > QuantidadeEstoque)
            throw new RegraDeNegocioException(
                DomainErrors.Produto.EstoqueInsuficiente(Nome, QuantidadeEstoque, quantidade));

        QuantidadeEstoque -= quantidade;
    }

    public void ReporEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new RegraDeNegocioException(
                Error.Validation("QUANTIDADE_INVALIDA", "A quantidade deve ser maior que zero."));

        QuantidadeEstoque += quantidade;
    }
}
