using SmartExpense.Core.Enums;

namespace SmartExpense.Core.Models;

public class TransactionQueryParameters
{
    private const int MaxPageSize = 50;
    private int _pageSize = 10;

    public int PageNumber { get; set; } = 1;
    
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value;
    }

    public string? SearchTerm { get; set; }
    public int? CategoryId { get; set; }
    public TransactionType? TransactionType { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public string SortBy { get; set; } = "TransactionDate";
    public bool SortDescending { get; set; } = true;
}