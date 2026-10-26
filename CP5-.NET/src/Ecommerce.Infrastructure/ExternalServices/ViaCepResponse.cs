using System.Text.Json.Serialization;

namespace Ecommerce.Infrastructure.ExternalServices;

public class ViaCepResponse
{
    [JsonPropertyName("cep")]
    public string? Cep { get; set; }

    [JsonPropertyName("logradouro")]
    public string? Logradouro { get; set; }

    [JsonPropertyName("bairro")]
    public string? Bairro { get; set; }

    [JsonPropertyName("localidade")]
    public string? Localidade { get; set; }

    [JsonPropertyName("uf")]
    public string? Uf { get; set; }

    [JsonPropertyName("erro")]
    public object? Erro { get; set; }

    public bool CepInexistente =>
        Erro is not null && Erro.ToString()?.Equals("true", StringComparison.OrdinalIgnoreCase) == true;
}
