using System.ComponentModel.DataAnnotations;

namespace SmartExpense.Application.Dtos.Budget;

public class BudgetUpdateDto
{
    [Required(ErrorMessage = "Amount is required")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }
}