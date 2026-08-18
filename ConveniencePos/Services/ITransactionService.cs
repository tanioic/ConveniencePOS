using ConveniencePos.Models;

namespace ConveniencePos.Services;

public interface ITransactionService
{
    Task<Transaction> SaveTransactionAsync(
        decimal totalAmount,
        decimal taxAmount,
        IReadOnlyList<TransactionItem> items,
        CancellationToken cancellationToken = default);
}
