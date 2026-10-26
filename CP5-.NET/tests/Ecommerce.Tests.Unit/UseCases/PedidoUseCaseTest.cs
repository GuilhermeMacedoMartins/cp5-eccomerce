using Ecommerce.Application.DTOs.Pedido;
using Ecommerce.Application.UseCases;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Exceptions;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace Ecommerce.Tests.Unit.UseCases;

public class PedidoUseCaseTest
{
    private readonly Mock<IPedidoRepository> _pedidoRepository = new();
    private readonly Mock<IProdutoRepository> _produtoRepository = new();
    private readonly Mock<IUsuarioRepository> _usuarioRepository = new();
    private readonly Mock<IEnderecoService> _enderecoService = new();
    private readonly PedidoUseCase _useCase;

    public PedidoUseCaseTest()
    {
        _useCase = new PedidoUseCase(
            _pedidoRepository.Object,
            _produtoRepository.Object,
            _usuarioRepository.Object,
            _enderecoService.Object,
            NullLogger<PedidoUseCase>.Instance);

        _usuarioRepository
            .Setup(r => r.ObterPorIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new UsuarioEntity { Id = 1, Nome = "Joao", Email = "joao@teste.com" }));

        _enderecoService
            .Setup(s => s.ConsultarPorCepAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new EnderecoConsultado("01001-000", "Praca da Se", "Se", "Sao Paulo", "SP")));
    }

    private static CriarPedidoDto DtoValido(int produtoId = 1, int quantidade = 2) => new()
    {
        Cep = "01001000",
        Itens = [new CriarItemPedidoDto { ProdutoId = produtoId, Quantidade = quantidade }]
    };

    [Fact]
    [Trait("UseCase", "Pedidos")]
    public async Task ObterPorId_DeveConverterExcecaoDoRepositorioEmOperationResult()
    {
        // Arrange
        _pedidoRepository
            .Setup(r => r.ObterPorIdAsync(99, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntidadeNaoEncontradaException(DomainErrors.Pedido.NaoEncontrado(99)));

        // Act
        var resultado = await _useCase.ObterPorIdAsync(99);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("PEDIDO_NAO_ENCONTRADO", resultado.Error.Code);
        Assert.Equal(ErrorType.NotFound, resultado.Error.Type);
    }

    [Fact]
    [Trait("UseCase", "Pedidos")]
    public async Task Criar_DeveConsultarOServicoExternoEBaixarOEstoque()
    {
        // Arrange
        var produto = new ProdutoEntity { Id = 1, Nome = "Mouse", Preco = 50m, QuantidadeEstoque = 10, Ativo = true };

        _produtoRepository
            .Setup(r => r.ObterPorIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<ProdutoEntity> { produto }));

        _produtoRepository
            .Setup(r => r.AtualizarVariosAsync(It.IsAny<IEnumerable<ProdutoEntity>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _pedidoRepository
            .Setup(r => r.AdicionarAsync(It.IsAny<PedidoEntity>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((PedidoEntity p, CancellationToken _) => { p.Id = 1; return p; });

        _pedidoRepository
            .Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((int _, CancellationToken __) =>
            {
                var pedido = new PedidoEntity { Id = 1, UsuarioId = 1, ClienteNome = "Joao", ValorTotal = 100m };
                pedido.DefinirEndereco("01001-000", "Praca da Se", "Se", "Sao Paulo", "SP");
                return pedido;
            });

        // Act
        var resultado = await _useCase.CriarAsync(DtoValido(), usuarioId: 1);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(8, produto.QuantidadeEstoque);
        Assert.Equal("Sao Paulo", resultado.Value.Endereco.Cidade);
        _enderecoService.Verify(s => s.ConsultarPorCepAsync("01001000", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("UseCase", "Pedidos")]
    public async Task Criar_DeveFalhar_QuandoOServicoExternoEstaIndisponivel()
    {
        // Arrange
        _enderecoService
            .Setup(s => s.ConsultarPorCepAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<EnderecoConsultado>(DomainErrors.Cep.ServicoIndisponivel()));

        // Act
        var resultado = await _useCase.CriarAsync(DtoValido(), usuarioId: 1);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal(ErrorType.External, resultado.Error.Type);
        _pedidoRepository.Verify(r => r.AdicionarAsync(It.IsAny<PedidoEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("UseCase", "Pedidos")]
    public async Task Criar_DeveFalhar_QuandoEstoqueInsuficiente()
    {
        // Arrange
        var produto = new ProdutoEntity { Id = 1, Nome = "Mouse", Preco = 50m, QuantidadeEstoque = 1, Ativo = true };

        _produtoRepository
            .Setup(r => r.ObterPorIdsAsync(It.IsAny<IEnumerable<int>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success(new List<ProdutoEntity> { produto }));

        // Act
        var resultado = await _useCase.CriarAsync(DtoValido(quantidade: 99), usuarioId: 1);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("ESTOQUE_INSUFICIENTE", resultado.Error.Code);
    }

    [Fact]
    [Trait("UseCase", "Pedidos")]
    public async Task Criar_DeveRejeitarItensDuplicados()
    {
        // Arrange
        var dto = new CriarPedidoDto
        {
            Cep = "01001000",
            Itens =
            [
                new CriarItemPedidoDto { ProdutoId = 1, Quantidade = 1 },
                new CriarItemPedidoDto { ProdutoId = 1, Quantidade = 2 }
            ]
        };

        // Act
        var resultado = await _useCase.CriarAsync(dto, usuarioId: 1);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("ITENS_DUPLICADOS", resultado.Error.Code);
    }

    [Fact]
    [Trait("UseCase", "Pedidos")]
    public async Task AtualizarStatus_DeveFalhar_QuandoPedidoJaEntregue()
    {
        // Arrange
        _pedidoRepository
            .Setup(r => r.ObterPorIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PedidoEntity { Id = 1, Status = StatusPedido.Entregue });

        // Act
        var resultado = await _useCase.AtualizarStatusAsync(1,
            new AtualizarStatusPedidoDto { Status = StatusPedido.Enviado });

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("STATUS_FINALIZADO", resultado.Error.Code);
    }
}
