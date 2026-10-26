using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Ecommerce.Infrastructure.ExternalServices;

public partial class ViaCepEnderecoService : IEnderecoService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepEnderecoService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public ViaCepEnderecoService(HttpClient httpClient, ILogger<ViaCepEnderecoService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    [GeneratedRegex(@"^\d{8}$")]
    private static partial Regex CepRegex();

    public async Task<Result<EnderecoConsultado>> ConsultarPorCepAsync(string cep, CancellationToken ct = default)
    {
        var limpo = new string((cep ?? string.Empty).Where(char.IsDigit).ToArray());

        if (!CepRegex().IsMatch(limpo))
            return Result.Failure<EnderecoConsultado>(DomainErrors.Cep.Invalido(cep ?? string.Empty));

        try
        {
            _logger.LogInformation("Consultando o CEP {Cep} no servico externo ViaCEP", limpo);

            using var response = await _httpClient.GetAsync($"ws/{limpo}/json/", ct);

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return Result.Failure<EnderecoConsultado>(DomainErrors.Cep.Invalido(limpo));

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("ViaCEP respondeu {StatusCode} para o CEP {Cep}", response.StatusCode, limpo);
                return Result.Failure<EnderecoConsultado>(DomainErrors.Cep.ServicoIndisponivel());
            }

            var conteudo = await response.Content.ReadAsStringAsync(ct);
            var dados = JsonSerializer.Deserialize<ViaCepResponse>(conteudo, JsonOptions);

            if (dados is null || dados.CepInexistente)
            {
                _logger.LogInformation("CEP {Cep} nao existe na base dos Correios", limpo);
                return Result.Failure<EnderecoConsultado>(DomainErrors.Cep.NaoEncontrado(limpo));
            }

            var endereco = new EnderecoConsultado(
                dados.Cep ?? limpo,
                dados.Logradouro ?? string.Empty,
                dados.Bairro ?? string.Empty,
                dados.Localidade ?? string.Empty,
                dados.Uf ?? string.Empty);

            _logger.LogInformation("CEP {Cep} resolvido para {Cidade}/{Uf}", limpo, endereco.Cidade, endereco.Uf);
            return Result.Success(endereco);
        }
        catch (TaskCanceledException ex) when (!ct.IsCancellationRequested)
        {
            _logger.LogError(ex, "Timeout ao consultar o CEP {Cep}", limpo);
            return Result.Failure<EnderecoConsultado>(DomainErrors.Cep.ServicoIndisponivel());
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha de rede ao consultar o CEP {Cep}", limpo);
            return Result.Failure<EnderecoConsultado>(DomainErrors.Cep.ServicoIndisponivel());
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Resposta invalida do ViaCEP para o CEP {Cep}", limpo);
            return Result.Failure<EnderecoConsultado>(DomainErrors.Cep.ServicoIndisponivel());
        }
    }
}
