using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.DTOs.Pedido;
using Ecommerce.Application.Mappings;
using Ecommerce.Application.UseCases.Interfaces;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.UseCases;

public class PedidoUseCase : IPedidoUseCase
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IEnderecoService _enderecoService;
    private readonly ILogger<PedidoUseCase> _logger;

    public PedidoUseCase(
        IPedidoRepository pedidoRepository,
        IProdutoRepository produtoRepository,
        IUsuarioRepository usuarioRepository,
        IEnderecoService enderecoService,
        ILogger<PedidoUseCase> logger)
    {
        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _usuarioRepository = usuarioRepository;
        _enderecoService = enderecoService;
        _logger = logger;
    }

    public async Task<Result<PageResult<PedidoDto>>> ObterPaginadoAsync(PageQueryDto query, int? usuarioId, CancellationToken ct = default)
    {
        try
        {
            var pagina = await _pedidoRepository.ObterPaginadoAsync(query.PageNumber, query.PageSize, usuarioId, ct);

            return Result.Success(new PageResult<PedidoDto>(
                pagina.Data.ToDtoList(),
                pagina.PageNumber,
                pagina.PageSize,
                pagina.TotalRecords));
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Falha ao listar pedidos");
            return Result.Failure<PageResult<PedidoDto>>(ex.Error);
        }
    }

    public async Task<Result<PedidoDto>> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id, ct);
            return Result.Success(pedido.ToDto());
        }
        catch (EntidadeNaoEncontradaException ex)
        {
            // Converte a excecao do repositorio em Result
            _logger.LogWarning("Pedido {PedidoId} nao encontrado", id);
            return Result.Failure<PedidoDto>(ex.Error);
        }
    }

    public async Task<Result<PedidoDto>> CriarAsync(CriarPedidoDto dto, int usuarioId, CancellationToken ct = default)
    {
        if (dto.Itens is null || dto.Itens.Count == 0)
            return Result.Failure<PedidoDto>(DomainErrors.Pedido.SemItens());

        var duplicados = dto.Itens.GroupBy(i => i.ProdutoId)
                                  .Where(g => g.Count() > 1)
                                  .Select(g => g.Key)
                                  .ToList();

        if (duplicados.Count != 0)
            return Result.Failure<PedidoDto>(DomainErrors.Pedido.ItensDuplicados(duplicados));

        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, ct);
        if (usuario.IsFailure)
            return Result.Failure<PedidoDto>(usuario.Error);

        var endereco = await _enderecoService.ConsultarPorCepAsync(dto.Cep, ct);
        if (endereco.IsFailure)
        {
            _logger.LogWarning("Servico externo de CEP falhou: {Codigo}", endereco.Error.Code);
            return Result.Failure<PedidoDto>(endereco.Error);
        }

        var ids = dto.Itens.Select(i => i.ProdutoId).Distinct().ToList();
        var produtosResult = await _produtoRepository.ObterPorIdsAsync(ids, ct);

        if (produtosResult.IsFailure)
            return Result.Failure<PedidoDto>(produtosResult.Error);

        var produtos = produtosResult.Value.ToDictionary(p => p.Id);

        var inexistente = ids.FirstOrDefault(id => !produtos.ContainsKey(id));
        if (inexistente != 0)
            return Result.Failure<PedidoDto>(DomainErrors.Produto.NaoEncontrado(inexistente));

        var pedido = new PedidoEntity
        {
            UsuarioId = usuarioId,
            ClienteNome = usuario.Value.Nome,
            ClienteEmail = usuario.Value.Email,
            DataPedido = DateTime.UtcNow,
            Status = StatusPedido.Pendente
        };

        pedido.DefinirEndereco(
            endereco.Value.Cep,
            endereco.Value.Logradouro,
            endereco.Value.Bairro,
            endereco.Value.Cidade,
            endereco.Value.Uf);

        try
        {
            foreach (var item in dto.Itens)
            {
                var produto = produtos[item.ProdutoId];

                produto.BaixarEstoque(item.Quantidade);

                pedido.Itens.Add(new ItemPedidoEntity
                {
                    ProdutoId = produto.Id,
                    Quantidade = item.Quantidade,
                    PrecoUnitario = produto.Preco
                });
            }
        }
        catch (RegraDeNegocioException ex)
        {
            return Result.Failure<PedidoDto>(ex.Error);
        }

        pedido.RecalcularValorTotal();

        try
        {
            var atualizacao = await _produtoRepository.AtualizarVariosAsync(produtos.Values, ct);
            if (atualizacao.IsFailure)
                return Result.Failure<PedidoDto>(atualizacao.Error);

            var criado = await _pedidoRepository.AdicionarAsync(pedido, ct);
            var completo = await _pedidoRepository.ObterPorIdAsync(criado.Id, ct);

            _logger.LogInformation("Pedido {PedidoId} criado para o usuario {UsuarioId} no valor de {Valor}",
                criado.Id, usuarioId, criado.ValorTotal);

            return Result.Success(completo.ToDto());
        }
        catch (DomainException ex)
        {
            _logger.LogError(ex, "Falha ao persistir o pedido");
            return Result.Failure<PedidoDto>(ex.Error);
        }
    }

    public async Task<Result<PedidoDto>> AtualizarStatusAsync(int id, AtualizarStatusPedidoDto dto, CancellationToken ct = default)
    {
        try
        {
            var pedido = await _pedidoRepository.ObterPorIdAsync(id, ct);

            pedido.AlterarStatus(dto.Status);

            // Cancelamento repoe o estoque
            if (dto.Status == StatusPedido.Cancelado)
            {
                var ids = pedido.Itens.Select(i => i.ProdutoId).ToList();
                var produtos = await _produtoRepository.ObterPorIdsAsync(ids, ct);

                if (produtos.IsSuccess)
                {
                    foreach (var item in pedido.Itens)
                    {
                        var produto = produtos.Value.FirstOrDefault(p => p.Id == item.ProdutoId);
                        produto?.ReporEstoque(item.Quantidade);
                    }

                    await _produtoRepository.AtualizarVariosAsync(produtos.Value, ct);
                }
            }

            await _pedidoRepository.AtualizarAsync(pedido, ct);

            _logger.LogInformation("Pedido {PedidoId} mudou para {Status}", id, dto.Status);
            return Result.Success(pedido.ToDto());
        }
        catch (DomainException ex)
        {
            return Result.Failure<PedidoDto>(ex.Error);
        }
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken ct = default)
    {
        try
        {
            await _pedidoRepository.RemoverAsync(id, ct);
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Error);
        }
    }
}
