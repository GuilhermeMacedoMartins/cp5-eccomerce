using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.DTOs.Produto;
using Ecommerce.Application.Mappings;
using Ecommerce.Application.UseCases.Interfaces;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Application.UseCases;

public class ProdutoUseCase : IProdutoUseCase
{
    private readonly IProdutoRepository _repository;
    private readonly ILogger<ProdutoUseCase> _logger;

    public ProdutoUseCase(IProdutoRepository repository, ILogger<ProdutoUseCase> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Result<PageResult<ProdutoDto>>> ObterPaginadoAsync(PageQueryDto query, CancellationToken ct = default)
    {
        _logger.LogInformation("Listando produtos. Pagina {PageNumber}, tamanho {PageSize}, categoria {Categoria}",
            query.PageNumber, query.PageSize, query.Categoria ?? "todas");

        var resultado = await _repository.ObterPaginadoAsync(query.PageNumber, query.PageSize, query.Categoria, ct);

        if (resultado.IsFailure)
        {
            _logger.LogWarning("Falha ao listar produtos: {Codigo}", resultado.Error.Code);
            return Result.Failure<PageResult<ProdutoDto>>(resultado.Error);
        }

        var pagina = resultado.Value;

        return Result.Success(new PageResult<ProdutoDto>(
            pagina.Data.ToDtoList(),
            pagina.PageNumber,
            pagina.PageSize,
            pagina.TotalRecords));
    }

    public async Task<Result<ProdutoDto>> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var resultado = await _repository.ObterPorIdAsync(id, ct);

        return resultado.IsFailure
            ? Result.Failure<ProdutoDto>(resultado.Error)
            : Result.Success(resultado.Value.ToDto());
    }

    public async Task<Result<ProdutoDto>> CriarAsync(CriarProdutoDto dto, CancellationToken ct = default)
    {
        // Valida antes de tocar o banco
        if (dto.Preco <= 0)
            return Result.Failure<ProdutoDto>(DomainErrors.Produto.PrecoInvalido());

        var resultado = await _repository.AdicionarAsync(dto.ToEntity(), ct);

        if (resultado.IsFailure)
            return Result.Failure<ProdutoDto>(resultado.Error);

        _logger.LogInformation("Produto {ProdutoId} criado", resultado.Value.Id);
        return Result.Success(resultado.Value.ToDto());
    }

    public async Task<Result<ProdutoDto>> AtualizarAsync(int id, AtualizarProdutoDto dto, CancellationToken ct = default)
    {
        var busca = await _repository.ObterPorIdAsync(id, ct);

        if (busca.IsFailure)
            return Result.Failure<ProdutoDto>(busca.Error);

        var produto = busca.Value;
        dto.ApplyTo(produto);

        var atualizacao = await _repository.AtualizarAsync(produto, ct);

        return atualizacao.IsFailure
            ? Result.Failure<ProdutoDto>(atualizacao.Error)
            : Result.Success(produto.ToDto());
    }

    public async Task<Result> RemoverAsync(int id, CancellationToken ct = default)
    {
        var resultado = await _repository.RemoverAsync(id, ct);

        if (resultado.IsSuccess)
            _logger.LogInformation("Produto {ProdutoId} removido", id);

        return resultado;
    }
}
