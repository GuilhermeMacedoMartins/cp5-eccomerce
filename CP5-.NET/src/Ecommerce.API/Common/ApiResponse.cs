using Ecommerce.Domain.Common;

namespace Ecommerce.API.Common;

public class ApiResponse<T>
{
    public bool Sucesso { get; set; }
    public T? Dados { get; set; }
    public ApiError? Erro { get; set; }
    public List<Link> Links { get; set; } = new();

    public static ApiResponse<T> Ok(T dados) => new() { Sucesso = true, Dados = dados };

    public static ApiResponse<T> Falha(Error error) => new()
    {
        Sucesso = false,
        Erro = new ApiError { Codigo = error.Code, Mensagem = error.Message, Tipo = error.Type.ToString() }
    };
}

public class ApiError
{
    public string Codigo { get; set; } = string.Empty;
    public string Mensagem { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string? TraceId { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
