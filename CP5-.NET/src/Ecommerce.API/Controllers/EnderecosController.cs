using Ecommerce.API.Common;
using Ecommerce.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Ecommerce.API.Controllers;

[Route("api/[controller]")]
[Authorize]
[SwaggerTag("Consulta de enderecos no servico externo ViaCEP.")]
public class EnderecosController : ApiControllerBase
{
    private readonly IEnderecoService _enderecoService;

    public EnderecosController(IEnderecoService enderecoService)
    {
        _enderecoService = enderecoService;
    }

    [HttpGet("{cep}")]
    [SwaggerOperation(
        Summary = "Consulta um CEP no servico externo",
        Description = "Chama a API publica do ViaCEP com retry e circuit breaker. " +
                      "CEP invalido devolve 400, inexistente devolve 404 e servico fora do ar devolve 503.")]
    [SwaggerResponse(StatusCodes.Status200OK, "Endereco encontrado", typeof(ApiResponse<EnderecoConsultado>))]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "CEP mal formatado", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "CEP inexistente", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status503ServiceUnavailable, "Servico externo indisponivel", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Consultar(string cep, CancellationToken ct)
    {
        var resultado = await _enderecoService.ConsultarPorCepAsync(cep, ct);

        return resultado.IsFailure
            ? TratarFalha(resultado.Error)
            : Ok(ApiResponse<EnderecoConsultado>.Ok(resultado.Value));
    }
}
