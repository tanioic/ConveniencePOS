using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConveniencePos.Models;

/// <summary>
/// 取引概要。会計確定時に1レコード保存される。
/// </summary>
public class Transaction
{
    /// <summary>取引ID（主キー、自動採番）</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>取引日時（UTC）</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>税込合計金額</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    /// <summary>消費税合計額</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal TaxAmount { get; set; }

    /// <summary>取引明細コレクション</summary>
    public ICollection<TransactionItem> Items { get; set; } = new List<TransactionItem>();
}
