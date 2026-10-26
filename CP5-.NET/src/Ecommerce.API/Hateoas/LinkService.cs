using Ecommerce.Application.DTOs.Common;
using Ecommerce.Application.DTOs.Pedido;
using Ecommerce.Application.DTOs.Produto;
using Ecommerce.Domain.Common;

namespace Ecommerce.API.Hateoas;

public class LinkService : ILinkService
{
    private readonly IHttpContextAccessor _accessor;

    public LinkService(IHttpContextAccessor accessor)
    {
        _accessor = accessor;
    }

    private string BaseUrl
    {
        get
        {
            var request = _accessor.HttpContext?.Request;
            return request is null ? string.Empty : $"{request.Scheme}://{request.Host}";
        }
    }

    public ProdutoDto AdicionarLinks(ProdutoDto produto, bool ehAdministrador)
    {
        produto.Links.Clear();

        produto.Links.Add(new Link($"{BaseUrl}/api/produtos/{produto.Id}", "self", "GET"));
        produto.Links.Add(new Link($"{BaseUrl}/api/produtos", "collection", "GET"));

        // So aparecem para quem pode escrever
        if (ehAdministrador)
        {
            produto.Links.Add(new Link($"{BaseUrl}/api/produtos/{produto.Id}", "update", "PUT"));
            produto.Links.Add(new Link($"{BaseUrl}/api/produtos/{produto.Id}", "delete", "DELETE"));
        }

        return produto;
    }

    public PedidoDto AdicionarLinks(PedidoDto pedido)
    {
        pedido.Links.Clear();

        pedido.Links.Add(new Link($"{BaseUrl}/api/pedidos/{pedido.Id}", "self", "GET"));
        pedido.Links.Add(new Link($"{BaseUrl}/api/pedidos", "collection", "GET"));

        // Depende do estado do pedido
        var finalizado = pedido.Status is "Entregue" or "Cancelado";

        if (!finalizado)
        {
            pedido.Links.Add(new Link($"{BaseUrl}/api/pedidos/{pedido.Id}/status", "update-status", "PUT"));
            pedido.Links.Add(new Link($"{BaseUrl}/api/pedidos/{pedido.Id}/status", "cancel", "PUT"));
        }

        pedido.Links.Add(new Link($"{BaseUrl}/api/pedidos/{pedido.Id}", "delete", "DELETE"));

        return pedido;
    }

    public PageResult<T> AdicionarLinksDaColecao<T>(PageResult<T> pagina, string rotaBase, PageQueryDto query)
    {
        pagina.Links.Clear();

        var categoria = string.IsNullOrWhiteSpace(query.Categoria) ? string.Empty : $"&categoria={query.Categoria}";

        string Url(int numero) =>
            $"{BaseUrl}{rotaBase}?pageNumber={numero}&pageSize={pagina.PageSize}{categoria}";

        pagina.Links.Add(new Link(Url(pagina.PageNumber), "self", "GET"));

        if (pagina.TotalPages > 0)
        {
            pagina.Links.Add(new Link(Url(1), "first", "GET"));
            pagina.Links.Add(new Link(Url(pagina.TotalPages), "last", "GET"));
        }

        if (pagina.HasPrevious)
            pagina.Links.Add(new Link(Url(pagina.PageNumber - 1), "prev", "GET"));

        if (pagina.HasNext)
            pagina.Links.Add(new Link(Url(pagina.PageNumber + 1), "next", "GET"));

        return pagina;
    }
}
