using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.DTOs.Produto;
using Ecommerce.Domain.Common;

namespace Ecommerce.Application.UseCases.Interfaces;

public interface IProdutoUseCase
{
    Task<Result<PageResult<ProdutoDto>>> ObterPaginadoAsync(PageQueryDto query, CancellationToken ct = default);
    Task<Result<ProdutoDto>> ObterPorIdAsync(int id, CancellationToken ct = default);
    Task<Result<ProdutoDto>> CriarAsync(CriarProdutoDto dto, CancellationToken ct = default);
    Task<Result<ProdutoDto>> AtualizarAsync(int id, AtualizarProdutoDto dto, CancellationToken ct = default);
    Task<Result> RemoverAsync(int id, CancellationToken ct = default);
}
