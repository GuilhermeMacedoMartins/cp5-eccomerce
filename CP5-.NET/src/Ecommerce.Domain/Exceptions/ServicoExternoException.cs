using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Exceptions;

public sealed class ServicoExternoException : DomainException
{
    public ServicoExternoException(string servico, Error error) : base(error)
    {
        Servico = servico;
    }

    public string Servico { get; }
}
