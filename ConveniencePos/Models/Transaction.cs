namespace ConveniencePos.Models;

public class Transaction
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxAmount { get; set; }

    public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
}
