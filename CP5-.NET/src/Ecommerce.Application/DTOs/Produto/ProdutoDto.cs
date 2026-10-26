using Ecommerce.Domain.Common;

namespace Ecommerce.Application.DTOs.Produto;

public class ProdutoDto : IHateoasResource
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Descricao { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int QuantidadeEstoque { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public bool Ativo { get; set; }
    public DateTime DataCriacao { get; set; }

    public List<Link> Links { get; set; } = new();
}
