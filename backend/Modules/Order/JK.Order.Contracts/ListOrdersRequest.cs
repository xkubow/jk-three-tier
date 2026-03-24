namespace JK.Order.Contracts;

public class ListOrdersRequest
{
    public string? SearchTerm { get; set; }

    public int Page { get; set; } = 1;

    public int PageSize { get; set; } = 20;

    public string? SortBy { get; set; }

    public string SortDirection { get; set; } = "asc";
}

