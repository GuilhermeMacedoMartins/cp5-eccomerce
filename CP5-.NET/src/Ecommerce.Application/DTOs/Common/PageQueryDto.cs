using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs.Common;

public class PageQueryDto
{
    public const int TamanhoMaximoPagina = 50;

    private int _pageNumber = 1;
    private int _pageSize = 10;

    [Range(1, int.MaxValue)]
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    [Range(1, TamanhoMaximoPagina)]
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : Math.Min(value, TamanhoMaximoPagina);
    }

    public string? Categoria { get; set; }
}
