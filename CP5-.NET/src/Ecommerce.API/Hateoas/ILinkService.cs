using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.DTOs.Pedido;
using Ecommerce.Application.DTOs.Produto;
using Ecommerce.Domain.Common;

namespace Ecommerce.API.Hateoas;

public interface ILinkService
{
    ProdutoDto AdicionarLinks(ProdutoDto produto, bool ehAdministrador);
    PedidoDto AdicionarLinks(PedidoDto pedido);
    PageResult<T> AdicionarLinksDaColecao<T>(PageResult<T> pagina, string rotaBase, PageQueryDto query);
}
