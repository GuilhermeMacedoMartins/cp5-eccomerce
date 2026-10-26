using Ecommerce.Infrastructure.Security;

namespace Ecommerce.Tests.Unit.Security;

public class PasswordHasherTest
{
    private readonly PasswordHasher _hasher = new();

    [Fact]
    [Trait("Seguranca", "Hash")]
    public void GerarHash_DeveProduzirSaltDiferenteParaSenhasIguais()
    {
        // Arrange & Act
        var (hash1, salt1) = _hasher.GerarHash("MesmaSenha@1");
        var (hash2, salt2) = _hasher.GerarHash("MesmaSenha@1");

        // Assert
        Assert.NotEqual(salt1, salt2);
        Assert.NotEqual(hash1, hash2);
    }

    [Fact]
    [Trait("Seguranca", "Hash")]
    public void Verificar_DeveAceitarASenhaCorreta()
    {
        // Arrange
        var (hash, salt) = _hasher.GerarHash("Senha@123");

        // Act & Assert
        Assert.True(_hasher.Verificar("Senha@123", hash, salt));
    }

    [Fact]
    [Trait("Seguranca", "Hash")]
    public void Verificar_DeveRecusarSenhaErradaOuHashCorrompido()
    {
        // Arrange
        var (hash, salt) = _hasher.GerarHash("Senha@123");

        // Act & Assert
        Assert.False(_hasher.Verificar("outra", hash, salt));
        Assert.False(_hasher.Verificar("Senha@123", "nao-e-base64!!", salt));
    }
}
