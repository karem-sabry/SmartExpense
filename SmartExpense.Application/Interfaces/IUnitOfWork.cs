namespace SmartExpense.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    ICategoryRepository Categories { get; }
    ITransactionRepository Transactions { get; }
    IBudgetRepository Budgets { get; }
    IRecurringTransactionRepository RecurringTransactions { get; }
    Task<int> SaveChangesAsync();
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}