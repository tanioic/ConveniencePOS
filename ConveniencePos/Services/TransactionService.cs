using ConveniencePos.Data;
using ConveniencePos.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConveniencePos.Services;

public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<PosDbContext> _contextFactory;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(IDbContextFactory<PosDbContext> contextFactory, ILogger<TransactionService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Transaction> SaveTransactionAsync(
        decimal totalAmount,
        decimal taxAmount,
        IReadOnlyList<TransactionItem> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
            throw new ArgumentException("取引明細は1件以上必要です。", nameof(items));

        if (totalAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalAmount), totalAmount, "合計金額は0以上である必要があります。");

        if (taxAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(taxAmount), taxAmount, "消費税額は0以上である必要があります。");

        var transaction = new Transaction
        {
            CreatedAt = DateTime.UtcNow,
            TotalAmount = totalAmount,
            TaxAmount = taxAmount,
            Items = items.ToList()
        };

        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Transactions.Add(transaction);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("取引 TRX-{TransactionId} を保存しました (合計: {TotalAmount})", transaction.Id, totalAmount);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "取引保存に失敗しました (合計: {TotalAmount})", totalAmount);
            throw new InvalidOperationException(
                "取引の保存に失敗しました。データベース接続を確認してください。", ex);
        }

        return transaction;
    }
}
