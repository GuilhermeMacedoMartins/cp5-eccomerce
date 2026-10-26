using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.DTOs.Pedido;
using Ecommerce.Domain.Common;

namespace Ecommerce.Application.UseCases.Interfaces;

public interface IPedidoUseCase
{
    Task<Result<PageResult<PedidoDto>>> ObterPaginadoAsync(PageQueryDto query, int? usuarioId, CancellationToken ct = default);
    Task<Result<PedidoDto>> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Result<PedidoDto>> CriarAsync(CriarPedidoDto dto, int usuarioId, CancellationToken ct = default);
    Task<Result<PedidoDto>> AtualizarStatusAsync(int id, AtualizarStatusPedidoDto dto, CancellationToken ct = default);
    Task<Result> RemoverAsync(int id, CancellationToken ct = default);
}
