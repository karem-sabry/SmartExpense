using System.ComponentModel.DataAnnotations;
using SmartExpense.Core.Enums;

namespace SmartExpense.Application.Dtos.Transaction;

public class TransactionUpdateDto
{
    [Required(ErrorMessage = "Category is required")]
    public int CategoryId { get; set; }

    [Required(ErrorMessage = "Description is required")]
    [StringLength(200, MinimumLength = 1, ErrorMessage = "Description must be between 1 and 200 characters")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Transaction type is required")]
    public TransactionType TransactionType { get; set; }

    [Required(ErrorMessage = "Transaction date is required")]
    public DateTime TransactionDate { get; set; }

    [StringLength(500, ErrorMessage = "Notes cannot exceed 500 characters")]
    public string? Notes { get; set; }
}