using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IPedidoRepository
{
    Task<PageResult<PedidoEntity>> ObterPaginadoAsync(int pageNumber, int pageSize, int? usuarioId, CancellationToken ct = default);

    Task<PedidoEntity> ObterPorIdAsync(int id, CancellationToken ct = default);

    Task<PedidoEntity> AdicionarAsync(PedidoEntity pedido, CancellationToken ct = default);
    Task AtualizarAsync(PedidoEntity pedido, CancellationToken ct = default);
    Task RemoverAsync(int id, CancellationToken ct = default);
}
