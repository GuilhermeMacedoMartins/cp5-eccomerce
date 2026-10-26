using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.DTOs.Produto;
using Ecommerce.Application.UseCases;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Ecommerce.Tests.Unit.UseCases;

public class ProdutoUseCaseTest
{
    private readonly Mock<IProdutoRepository> _repository = new();
    private readonly ProdutoUseCase _useCase;

    public ProdutoUseCaseTest()
    {
        _useCase = new ProdutoUseCase(_repository.Object, NullLogger<ProdutoUseCase>.Instance);
    }

    [Fact]
    [Trait("UseCase", "Produtos")]
    public async Task ObterPaginado_DeveMontarOPageResultComMetadados()
    {
        // Arrange
        var produtos = new List<ProdutoEntity>
        {
            new() { Id = 1, Nome = "Mouse", Preco = 50m, Categoria = "Perifericos" },
            new() { Id = 2, Nome = "Teclado", Preco = 120m, Categoria = "Perifericos" }
        };

        _repository
            .Setup(r => r.ObterPaginadoAsync(1, 2, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new PageResult<ProdutoEntity>(produtos, 1, 2, 5)));

        // Act
        var resultado = await _useCase.ObterPaginadoAsync(new PageQueryDto { PageNumber = 1, PageSize = 2 });

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(2, resultado.Value.Data.Count);
        Assert.Equal(5, resultado.Value.TotalRecords);
        Assert.Equal(3, resultado.Value.TotalPages);
        Assert.True(resultado.Value.HasNext);
    }

    [Fact]
    [Trait("UseCase", "Produtos")]
    public async Task ObterPorId_DevePropagarOErroRetornadoPeloRepositorio()
    {
        // Arrange
        _repository
            .Setup(r => r.ObterPorIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<ProdutoEntity>(DomainErrors.Produto.NaoEncontrado(99)));

        // Act
        var resultado = await _useCase.ObterPorIdAsync(99);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("PRODUTO_NAO_ENCONTRADO", resultado.Error.Code);
        Assert.Equal(ErrorType.NotFound, resultado.Error.Type);
    }

    [Fact]
    [Trait("UseCase", "Produtos")]
    public async Task Criar_DeveRejeitarPrecoZero_SemChamarORepositorio()
    {
        // Arrange
        var dto = new CriarProdutoDto { Nome = "Item", Preco = 0m, Categoria = "Geral" };

        // Act
        var resultado = await _useCase.CriarAsync(dto);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("PRECO_INVALIDO", resultado.Error.Code);
        _repository.Verify(r => r.AdicionarAsync(It.IsAny<ProdutoEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("UseCase", "Produtos")]
    public async Task Criar_DeveMapearEDevolverOProdutoCriado()
    {
        // Arrange
        var dto = new CriarProdutoDto { Nome = "Webcam", Preco = 250m, QuantidadeEstoque = 4, Categoria = "Video" };

        _repository
            .Setup(r => r.AdicionarAsync(It.IsAny<ProdutoEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProdutoEntity p, CancellationToken _) => { p.Id = 10; return Result.Success(p); });

        // Act
        var resultado = await _useCase.CriarAsync(dto);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(10, resultado.Value.Id);
        Assert.Equal("Webcam", resultado.Value.Nome);
        Assert.True(resultado.Value.Ativo);
    }

    [Fact]
    [Trait("UseCase", "Produtos")]
    public async Task Remover_DevePropagarConflitoDeProdutoVinculado()
    {
        // Arrange
        _repository
            .Setup(r => r.RemoverAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(DomainErrors.Produto.VinculadoAPedido(3)));

        // Act
        var resultado = await _useCase.RemoverAsync(3);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal(ErrorType.Conflict, resultado.Error.Type);
    }
}
