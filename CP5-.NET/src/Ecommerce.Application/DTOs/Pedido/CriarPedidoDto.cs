using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs.Pedido;

public class CriarPedidoDto
{
    [Required(ErrorMessage = "O CEP de entrega e obrigatorio.")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "Informe o CEP no formato 00000-000 ou 00000000.")]
    public string Cep { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "O pedido deve conter ao menos um item.")]
    public List<CriarItemPedidoDto> Itens { get; set; } = new();
}

public class CriarItemPedidoDto
{
    [Range(1, int.MaxValue, ErrorMessage = "O ProdutoId e obrigatorio.")]
    public int ProdutoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}
