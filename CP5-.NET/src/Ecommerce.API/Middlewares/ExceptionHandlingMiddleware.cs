using System.Text.Json;
using Ecommerce.API.Common;
using Ecommerce.Domain.Common;
using Ecommerce.Domain.Exceptions;

namespace Ecommerce.API.Middlewares;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (DomainException ex)
        {
            _logger.LogWarning(ex, "Erro de dominio {Codigo} em {Path}", ex.Error.Code, context.Request.Path);
            await EscreverAsync(context, ex.Error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro nao tratado em {Path}", context.Request.Path);
            await EscreverAsync(context, Error.Unexpected("ERRO_INTERNO",
                "Ocorreu um erro interno. Consulte os logs para mais detalhes."));
        }
    }

    private static async Task EscreverAsync(HttpContext context, Error error)
    {
        if (context.Response.HasStarted)
            return;

        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            ErrorType.External => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.Clear();
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = status;

        var resposta = ApiResponse<object>.Falha(error);
        if (resposta.Erro is not null)
            resposta.Erro.TraceId = context.TraceIdentifier;

        await context.Response.WriteAsync(JsonSerializer.Serialize(resposta, JsonOptions));
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandlingMiddleware(this IApplicationBuilder app) =>
        app.UseMiddleware<ExceptionHandlingMiddleware>();
}
