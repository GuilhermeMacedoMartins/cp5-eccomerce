using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Interfaces;

public interface IEnderecoService
{
    Task<Result<EnderecoConsultado>> ConsultarPorCepAsync(string cep, CancellationToken ct = default);
}

public sealed record EnderecoConsultado(
    string Cep,
    string Logradouro,
    string Bairro,
    string Cidade,
    string Uf);
