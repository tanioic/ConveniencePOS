namespace ConveniencePos.Services;

public record ReceiptItem(string Name, int Quantity, decimal LineTotalWithTax, int TaxRate);

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

public interface IReceiptService
{
    string GenerateReceipt(ReceiptContext context);
    Task SaveReceiptAsync(int transactionId, string receiptContent, CancellationToken cancellationToken = default);
}
