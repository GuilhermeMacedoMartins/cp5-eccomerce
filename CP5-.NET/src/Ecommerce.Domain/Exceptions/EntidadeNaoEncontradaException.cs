using Ecommerce.Domain.Common;

namespace Ecommerce.Domain.Exceptions;

public sealed class EntidadeNaoEncontradaException : DomainException
{
    public EntidadeNaoEncontradaException(Error error) : base(error) { }

    public EntidadeNaoEncontradaException(string entidade, int id)
        : base(Error.NotFound($"{entidade.ToUpperInvariant()}_NAO_ENCONTRADO",
                              $"{entidade} {id} nao foi encontrado.")) { }
}
