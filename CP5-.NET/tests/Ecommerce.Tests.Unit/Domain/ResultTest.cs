using Ecommerce.Domain.Common;

namespace Ecommerce.Tests.Unit.Domain;

public class ResultTest
{
    [Fact]
    [Trait("Dominio", "OperationResult")]
    public void Success_DeveIndicarSucessoSemErro()
    {
        // Arrange & Act
        var resultado = Result.Success();

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.False(resultado.IsFailure);
        Assert.Equal(Error.None, resultado.Error);
    }

    [Fact]
    [Trait("Dominio", "OperationResult")]
    public void SuccessComValor_DeveExporOPayload()
    {
        // Arrange
        const string esperado = "payload";

        // Act
        var resultado = Result.Success(esperado);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(esperado, resultado.Value);
    }

    [Fact]
    [Trait("Dominio", "OperationResult")]
    public void Failure_DeveCarregarOErroCustomizado()
    {
        // Arrange
        var erro = DomainErrors.Produto.NaoEncontrado(42);

        // Act
        var resultado = Result.Failure<string>(erro);

        // Assert
        Assert.True(resultado.IsFailure);
        Assert.Equal("PRODUTO_NAO_ENCONTRADO", resultado.Error.Code);
        Assert.Equal(ErrorType.NotFound, resultado.Error.Type);
    }

    [Fact]
    [Trait("Dominio", "OperationResult")]
    public void Value_DeveLancarQuandoOResultadoForFalha()
    {
        // Arrange
        var resultado = Result.Failure<string>(DomainErrors.Produto.NaoEncontrado(1));

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => _ = resultado.Value);
    }
}
