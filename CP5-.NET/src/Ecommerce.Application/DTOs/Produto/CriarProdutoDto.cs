using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs.Produto;

public class CriarProdutoDto
{
    [Required(ErrorMessage = "O nome do produto e obrigatorio.")]
    [StringLength(150, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "O preco deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "O estoque nao pode ser negativo.")]
    public int QuantidadeEstoque { get; set; }

    [Required(ErrorMessage = "A categoria e obrigatoria.")]
    [StringLength(80)]
    public string Categoria { get; set; } = string.Empty;
}
