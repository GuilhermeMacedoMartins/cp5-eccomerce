using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Exceptions;

public abstract class DomainException : Exception
{
    protected DomainException(Error error) : base(error.Message)
    {
        Error = error;
    }

    public Error Error { get; }
}
