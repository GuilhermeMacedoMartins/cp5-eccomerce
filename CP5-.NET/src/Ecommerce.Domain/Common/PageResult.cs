namespace Ecommerce.Domain.Common;

public class PageResult<T>
{
    public PageResult() { }

    public PageResult(IReadOnlyList<T> data, int pageNumber, int pageSize, int totalRecords)
    {
        Data = data;
        PageNumber = pageNumber;
        PageSize = pageSize;
        TotalRecords = totalRecords;
    }

    public IReadOnlyList<T> Data { get; set; } = new List<T>();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalRecords / (double)PageSize);
    public int ItemsOnPage => Data.Count;
    public bool HasPrevious => PageNumber > 1;
    public bool HasNext => PageNumber < TotalPages;
    public bool IsFirstPage => PageNumber == 1;
    public bool IsLastPage => PageNumber >= TotalPages;

    public List<Link> Links { get; set; } = new();
}
