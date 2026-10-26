using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IProdutoRepository
{
    Task<Result<PageResult<ProdutoEntity>>> ObterPaginadoAsync(int pageNumber, int pageSize, string? categoria, CancellationToken ct = default);
    Task<Result<ProdutoEntity>> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Result<List<ProdutoEntity>>> ObterPorIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<Result<ProdutoEntity>> AdicionarAsync(ProdutoEntity produto, CancellationToken ct = default);
    Task<Result> AtualizarAsync(ProdutoEntity produto, CancellationToken ct = default);
    Task<Result> AtualizarVariosAsync(IEnumerable<ProdutoEntity> produtos, CancellationToken ct = default);
    Task<Result> RemoverAsync(int id, CancellationToken ct = default);
}
