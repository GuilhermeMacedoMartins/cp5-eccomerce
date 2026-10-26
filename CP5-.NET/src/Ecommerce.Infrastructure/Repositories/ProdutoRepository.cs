using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly ApplicationContext _context;
    private readonly ILogger<ProdutoRepository> _logger;

    public ProdutoRepository(ApplicationContext context, ILogger<ProdutoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<PageResult<ProdutoEntity>>> ObterPaginadoAsync(
        int pageNumber, int pageSize, string? categoria, CancellationToken ct = default)
    {
        try
        {
            // AsNoTracking: leitura sem rastreamento
            var query = _context.Produtos.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(categoria))
                query = query.Where(p => p.Categoria == categoria);

            var total = await query.CountAsync(ct);

            var dados = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return Result.Success(new PageResult<ProdutoEntity>(dados, pageNumber, pageSize, total));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar produtos paginados");
            return Result.Failure<PageResult<ProdutoEntity>>(
                Error.Unexpected("FALHA_CONSULTA_PRODUTOS", "Nao foi possivel consultar os produtos."));
        }
    }

    public async Task<Result<ProdutoEntity>> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);

            // Erro retornado, nao lancado
            return produto is null
                ? Result.Failure<ProdutoEntity>(DomainErrors.Produto.NaoEncontrado(id))
                : Result.Success(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar o produto {ProdutoId}", id);
            return Result.Failure<ProdutoEntity>(
                Error.Unexpected("FALHA_CONSULTA_PRODUTO", "Nao foi possivel consultar o produto."));
        }
    }

    public async Task<Result<List<ProdutoEntity>>> ObterPorIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        try
        {
            var lista = ids.ToList();
            var produtos = await _context.Produtos.Where(p => lista.Contains(p.Id)).ToListAsync(ct);
            return Result.Success(produtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar produtos por ids");
            return Result.Failure<List<ProdutoEntity>>(
                Error.Unexpected("FALHA_CONSULTA_PRODUTOS", "Nao foi possivel consultar os produtos."));
        }
    }

    public async Task<Result<ProdutoEntity>> AdicionarAsync(ProdutoEntity produto, CancellationToken ct = default)
    {
        try
        {
            await _context.Produtos.AddAsync(produto, ct);
            await _context.SaveChangesAsync(ct);
            return Result.Success(produto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar o produto {Nome}", produto.Nome);
            return Result.Failure<ProdutoEntity>(
                Error.Unexpected("FALHA_CRIAR_PRODUTO", "Nao foi possivel cadastrar o produto."));
        }
    }

    public async Task<Result> AtualizarAsync(ProdutoEntity produto, CancellationToken ct = default)
    {
        try
        {
            _context.Produtos.Update(produto);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Conflito de concorrencia no produto {ProdutoId}", produto.Id);
            return Result.Failure(
                Error.Conflict("CONFLITO_CONCORRENCIA", "O produto foi alterado por outro processo."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar o produto {ProdutoId}", produto.Id);
            return Result.Failure(
                Error.Unexpected("FALHA_ATUALIZAR_PRODUTO", "Nao foi possivel atualizar o produto."));
        }
    }

    public async Task<Result> AtualizarVariosAsync(IEnumerable<ProdutoEntity> produtos, CancellationToken ct = default)
    {
        try
        {
            _context.Produtos.UpdateRange(produtos);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar produtos em lote");
            return Result.Failure(
                Error.Unexpected("FALHA_ATUALIZAR_PRODUTOS", "Nao foi possivel atualizar os produtos."));
        }
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var produto = await _context.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct);

            if (produto is null)
                return Result.Failure(DomainErrors.Produto.NaoEncontrado(id));

            var vinculado = await _context.ItensPedido.AnyAsync(i => i.ProdutoId == id, ct);
            if (vinculado)
                return Result.Failure(DomainErrors.Produto.VinculadoAPedido(id));

            _context.Produtos.Remove(produto);
            await _context.SaveChangesAsync(ct);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover o produto {ProdutoId}", id);
            return Result.Failure(
                Error.Unexpected("FALHA_REMOVER_PRODUTO", "Nao foi possivel remover o produto."));
        }
    }
}
