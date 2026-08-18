using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConveniencePos.Models;

public class TransactionItem
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    [ForeignKey(nameof(Transaction))]
    public int TransactionId { get; set; }

    [ForeignKey(nameof(Product))]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Range(8, 10)]
    public int AppliedTaxRate { get; set; }

    public Transaction? Transaction { get; set; }
    public Product? Product { get; set; }
}
