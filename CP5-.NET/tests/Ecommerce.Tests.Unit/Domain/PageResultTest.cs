using Ecommerce.Domain.Common;

namespace Ecommerce.Tests.Unit.Domain;

public class PageResultTest
{
    [Theory]
    [Trait("Dominio", "PageResult")]
    [InlineData(10, 3, 4)]
    [InlineData(10, 5, 2)]
    [InlineData(9, 3, 3)]
    [InlineData(0, 10, 0)]
    public void TotalPages_DeveArredondarParaCima(int totalRegistros, int tamanho, int paginasEsperadas)
    {
        // Arrange & Act
        var pagina = new PageResult<string>(new List<string>(), 1, tamanho, totalRegistros);

        // Assert
        Assert.Equal(paginasEsperadas, pagina.TotalPages);
    }

    [Fact]
    [Trait("Dominio", "PageResult")]
    public void PrimeiraPagina_NaoDeveTerAnteriorMasDeveTerProxima()
    {
        // Arrange
        var dados = new List<string> { "a", "b" };

        // Act
        var pagina = new PageResult<string>(dados, 1, 2, 10);

        // Assert
        Assert.False(pagina.HasPrevious);
        Assert.True(pagina.HasNext);
        Assert.True(pagina.IsFirstPage);
        Assert.False(pagina.IsLastPage);
        Assert.Equal(2, pagina.ItemsOnPage);
        Assert.Equal(10, pagina.TotalRecords);
    }

    [Fact]
    [Trait("Dominio", "PageResult")]
    public void UltimaPagina_DeveTerAnteriorENaoTerProxima()
    {
        // Arrange & Act
        var pagina = new PageResult<string>(new List<string> { "z" }, 5, 2, 10);

        // Assert
        Assert.True(pagina.HasPrevious);
        Assert.False(pagina.HasNext);
        Assert.True(pagina.IsLastPage);
    }
}
