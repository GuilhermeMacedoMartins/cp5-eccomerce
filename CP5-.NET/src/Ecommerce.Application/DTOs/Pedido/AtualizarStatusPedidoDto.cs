using System.ComponentModel.DataAnnotations;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs.Pedido;

public class AtualizarStatusPedidoDto
{
    [Required(ErrorMessage = "O novo status e obrigatorio.")]
    [EnumDataType(typeof(StatusPedido), ErrorMessage = "Status invalido.")]
    public StatusPedido Status { get; set; }
}
