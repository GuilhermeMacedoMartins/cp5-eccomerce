namespace Ecommerce.Domain.Common;

public sealed class Link
{
    public Link() { }

    public Link(string href, string rel, string method)
    {
        Href = href;
        Rel = rel;
        Method = method;
    }

    public string Href { get; set; } = string.Empty;

    public string Rel { get; set; } = string.Empty;

    public string Method { get; set; } = string.Empty;
}

public interface IHateoasResource
{
    List<Link> Links { get; set; }
}
