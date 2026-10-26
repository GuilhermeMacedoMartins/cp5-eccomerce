using System.Security.Cryptography;
using Ecommerce.Application.Interfaces;

namespace Ecommerce.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    private const int TamanhoSalt = 16;
    private const int TamanhoHash = 32;
    private const int Iteracoes = 100_000;

    public (string hash, string salt) GerarHash(string senha)
    {
        var salt = RandomNumberGenerator.GetBytes(TamanhoSalt);

        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password: senha,
            salt: salt,
            iterations: Iteracoes,
            hashAlgorithm: HashAlgorithmName.SHA256,
            outputLength: TamanhoHash);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }

    public bool Verificar(string senha, string hash, string salt)
    {
        try
        {
            var saltBytes = Convert.FromBase64String(salt);
            var hashEsperado = Convert.FromBase64String(hash);

            var hashInformado = Rfc2898DeriveBytes.Pbkdf2(
                password: senha,
                salt: saltBytes,
                iterations: Iteracoes,
                hashAlgorithm: HashAlgorithmName.SHA256,
                outputLength: TamanhoHash);

            // Comparacao em tempo constante
            return CryptographicOperations.FixedTimeEquals(hashEsperado, hashInformado);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
