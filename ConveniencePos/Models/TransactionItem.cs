using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConveniencePos.Models;

/// <summary>
/// 取引明細。商品ごとの数量・単価・適用税率を記録する。
/// </summary>
public class TransactionItem
{
    /// <summary>明細ID（主キー）</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>親取引ID（外部キー）</summary>
    public int TransactionId { get; set; }

    /// <summary>商品ID（外部キー）</summary>
    public int ProductId { get; set; }

    /// <summary>数量</summary>
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    /// <summary>販売時単価（税抜）</summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal UnitPrice { get; set; }

    /// <summary>購入時点の適用税率（8 or 10）</summary>
    [Range(0, 100)]
    public int AppliedTaxRate { get; set; }

    /// <summary>親取引ナビゲーションプロパティ</summary>
    public Transaction Transaction { get; set; } = null!;

    /// <summary>商品ナビゲーションプロパティ</summary>
    public Product Product { get; set; } = null!;
}
