using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Exceptions;

public sealed class PersistenciaException : DomainException
{
    public PersistenciaException(string operacao, Exception inner)
        : base(Error.Unexpected("FALHA_PERSISTENCIA", $"Falha ao {operacao} no banco de dados."))
    {
        OperacaoOriginal = operacao;
        InnerExceptionOriginal = inner;
    }

    public string OperacaoOriginal { get; }
    public Exception InnerExceptionOriginal { get; }
}
