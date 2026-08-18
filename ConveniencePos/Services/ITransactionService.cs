using ConveniencePos.Models;

namespace ConveniencePos.Services;

/// <summary>
/// 取引保存を行うサービスのインターフェース。
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// 取引をDBに保存する。
    /// </summary>
    /// <param name="totalAmount">税込合計金額。</param>
    /// <param name="taxAmount">消費税合計額。</param>
    /// <param name="items">取引明細リスト。1件以上必要。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    /// <returns>保存された取引オブジェクト。</returns>
    /// <exception cref="ArgumentNullException">items が null の場合。</exception>
    /// <exception cref="ArgumentException">items が空の場合。</exception>
    /// <exception cref="ArgumentOutOfRangeException">金額が負数の場合。</exception>
    /// <exception cref="InvalidOperationException">DB保存に失敗した場合。</exception>
    Task<Transaction> SaveTransactionAsync(
        decimal totalAmount,
        decimal taxAmount,
        IReadOnlyList<TransactionItem> items,
        CancellationToken cancellationToken = default);
}
