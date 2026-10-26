using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs.Produto;

public class AtualizarProdutoDto
{
    [Required]
    [StringLength(150, MinimumLength = 2)]
    public string Nome { get; set; } = string.Empty;

    [StringLength(500)]
    public string Descricao { get; set; } = string.Empty;

    [Range(0.01, 999999.99, ErrorMessage = "O preco deve ser maior que zero.")]
    public decimal Preco { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantidadeEstoque { get; set; }

    [Required]
    [StringLength(80)]
    public string Categoria { get; set; } = string.Empty;

    public bool Ativo { get; set; } = true;
}
