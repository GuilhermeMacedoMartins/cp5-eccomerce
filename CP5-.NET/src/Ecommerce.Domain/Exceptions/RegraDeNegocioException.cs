using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Exceptions;

public sealed class RegraDeNegocioException : DomainException
{
    public RegraDeNegocioException(Error error) : base(error) { }
}
