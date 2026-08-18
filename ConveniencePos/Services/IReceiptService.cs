namespace ConveniencePos.Services;

/// <summary>
/// レシート生成・保存を行うサービスのインターフェース。
/// </summary>
public interface IReceiptService
{
    /// <summary>
    /// レシートテキストを生成する。
    /// </summary>
    /// <param name="context">レシート生成に必要な情報。</param>
    /// <returns>固定幅フォーマットのレシートテキスト。</returns>
    string GenerateReceipt(ReceiptContext context);

    /// <summary>
    /// レシートをファイルに非同期保存する。
    /// </summary>
    /// <param name="transactionId">取引ID。ファイル名に使用される。</param>
    /// <param name="receiptContent">保存するレシートテキスト。</param>
    /// <param name="cancellationToken">キャンセルトークン。</param>
    Task SaveReceiptAsync(int transactionId, string receiptContent, CancellationToken cancellationToken = default);
}

/// <summary>
/// レシートに表示する1明細の情報。
/// </summary>
public record ReceiptItem(
    string Name,
    int Quantity,
    decimal LineTotalWithTax,
    int TaxRate);

/// <summary>
/// レシート生成に必要な全情報。
/// </summary>
public record ReceiptContext(
    int TransactionId,
    DateTime TransactionTime,
    IReadOnlyList<ReceiptItem> Items,
    decimal Subtotal,
    decimal TaxableAmount8,
    decimal TaxableAmount10,
    decimal TaxAmount8,
    decimal TaxAmount10,
    decimal TaxAmount,
    decimal TotalAmount,
    decimal ReceivedAmount,
    decimal Change);
