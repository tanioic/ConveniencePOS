namespace ConveniencePos.Services;

public interface IReceiptService
{
    string GenerateReceipt(
        int transactionId,
        DateTime transactionTime,
        IReadOnlyList<ReceiptItem> items,
        decimal subtotal,
        decimal taxableAmount8,
        decimal taxableAmount10,
        decimal taxAmount8,
        decimal taxAmount10,
        decimal taxAmount,
        decimal totalAmount,
        decimal receivedAmount,
        decimal change);

    void SaveReceipt(int transactionId, string receiptContent);
}

public record ReceiptItem(
    string Name,
    int Quantity,
    decimal LineTotalWithTax,
    int TaxRate);
