using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly ApplicationContext _context;
    private readonly ILogger<PedidoRepository> _logger;

    public PedidoRepository(ApplicationContext context, ILogger<PedidoRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PageResult<PedidoEntity>> ObterPaginadoAsync(
        int pageNumber, int pageSize, int? usuarioId, CancellationToken ct = default)
    {
        try
        {
            var query = _context.Pedidos.AsNoTracking();

            if (usuarioId.HasValue)
                query = query.Where(p => p.UsuarioId == usuarioId.Value);

            var total = await query.CountAsync(ct);

            // AsSplitQuery evita explosao cartesiana no join
            var dados = await query
                .Include(p => p.Itens).ThenInclude(i => i.Produto)
                .AsSplitQuery()
                .OrderByDescending(p => p.DataPedido)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return new PageResult<PedidoEntity>(dados, pageNumber, pageSize, total);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao consultar pedidos");
            throw new PersistenciaException("consultar pedidos", ex);
        }
    }

    public async Task<PedidoEntity> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Itens).ThenInclude(i => i.Produto)
            .AsSplitQuery()
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        // Erro lancado pelo repositorio
        if (pedido is null)
        {
            _logger.LogWarning("Pedido {PedidoId} nao encontrado", id);
            throw new EntidadeNaoEncontradaException(DomainErrors.Pedido.NaoEncontrado(id));
        }

        return pedido;
    }

    public async Task<PedidoEntity> AdicionarAsync(PedidoEntity pedido, CancellationToken ct = default)
    {
        try
        {
            await _context.Pedidos.AddAsync(pedido, ct);
            await _context.SaveChangesAsync(ct);
            _logger.LogInformation("Pedido {PedidoId} persistido", pedido.Id);
            return pedido;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar pedido");
            throw new PersistenciaException("criar o pedido", ex);
        }
    }

    public async Task AtualizarAsync(PedidoEntity pedido, CancellationToken ct = default)
    {
        try
        {
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar o pedido {PedidoId}", pedido.Id);
            throw new PersistenciaException("atualizar o pedido", ex);
        }
    }

    public async Task RemoverAsync(int id, CancellationToken ct = default)
    {
        var pedido = await _context.Pedidos.FirstOrDefaultAsync(p => p.Id == id, ct);

        if (pedido is null)
            throw new EntidadeNaoEncontradaException(DomainErrors.Pedido.NaoEncontrado(id));

        try
        {
            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync(ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover o pedido {PedidoId}", id);
            throw new PersistenciaException("remover o pedido", ex);
        }
    }
}
