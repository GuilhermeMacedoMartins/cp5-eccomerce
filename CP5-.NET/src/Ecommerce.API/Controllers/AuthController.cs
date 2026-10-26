using Ecommerce.API.Common;
using Ecommerce.Application.DTOs.Auth;
using Ecommerce.Application.UseCases.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace Ecommerce.API.Controllers;

[Route("api/[controller]")]
[AllowAnonymous]
[SwaggerTag("Autenticacao: registro de usuarios e emissao de token JWT.")]
public class AuthController : ApiControllerBase
{
    private readonly IAuthUseCase _authUseCase;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthUseCase authUseCase, ILogger<AuthController> logger)
    {
        _authUseCase = authUseCase;
        _logger = logger;
    }

    [HttpPost("login")]
    [SwaggerOperation(
        Summary = "Faz login e devolve o token JWT",
        Description = "Usuarios de demonstracao: admin@ecommerce.com / Admin@123 (Administrador) " +
                      "e cliente@ecommerce.com / Cliente@123 (Cliente).")]
    [SwaggerResponse(StatusCodes.Status200OK, "Autenticado com sucesso", typeof(ApiResponse<TokenDto>))]
    [SwaggerResponse(StatusCodes.Status403Forbidden, "Credenciais invalidas", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Login([FromBody] LoginDto dto, CancellationToken ct)
    {
        _logger.LogInformation("Tentativa de login");

        var resultado = await _authUseCase.LoginAsync(dto, ct);

        return resultado.IsFailure
            ? TratarFalha(resultado.Error)
            : Ok(ApiResponse<TokenDto>.Ok(resultado.Value));
    }

    [HttpPost("registrar")]
    [SwaggerOperation(
        Summary = "Registra um novo usuario",
        Description = "Cria a conta com perfil Cliente e ja devolve o token JWT.")]
    [SwaggerResponse(StatusCodes.Status201Created, "Usuario registrado", typeof(ApiResponse<TokenDto>))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "E-mail ja cadastrado", typeof(ApiResponse<object>))]
    public async Task<IActionResult> Registrar([FromBody] RegistrarUsuarioDto dto, CancellationToken ct)
    {
        var resultado = await _authUseCase.RegistrarAsync(dto, ct);

        return resultado.IsFailure
            ? TratarFalha(resultado.Error)
            : StatusCode(StatusCodes.Status201Created, ApiResponse<TokenDto>.Ok(resultado.Value));
    }
}
