using Ecommerce.API.Common;
using Ecommerce.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult TratarFalha(Error error)
    {
        var resposta = ApiResponse<object>.Falha(error);

        if (resposta.Erro is not null)
            resposta.Erro.TraceId = HttpContext.TraceIdentifier;

        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            ErrorType.External => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        return StatusCode(status, resposta);
    }

    protected int UsuarioId =>
        int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out var id) ? id : 0;

    protected bool EhAdministrador => User.IsInRole("Administrador");
}
