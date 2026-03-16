namespace JK.Configuration.Contracts;

public class ListConfigurationRequest
{
    public string? MarketCode { get; set; }
    public string? ServiceCode { get; set; }
    public string? SearchTerm { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string SortDirection { get; set; } = "asc";
}
