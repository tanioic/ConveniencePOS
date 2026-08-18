using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ConveniencePos.Models;

/// <summary>
/// 商品マスタ情報。JANコードで唯一に識別される。
/// </summary>
public class Product
{
    /// <summary>商品ID（主キー）</summary>
    [Key]
    public int Id { get; set; }

    /// <summary>JANコード（バーコード文字列、ユニーク）</summary>
    [Required]
    [MaxLength(20)]
    public string JanCode { get; set; } = string.Empty;

    /// <summary>商品名</summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>税抜価格</summary>
    [Column(TypeName = "decimal(18,2)")]
    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    /// <summary>税率（8: 軽減税率, 10: 標準税率）</summary>
    [Range(0, 100)]
    public int TaxRate { get; set; }
}
