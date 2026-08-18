using CommunityToolkit.Mvvm.ComponentModel;

namespace ConveniencePos.ViewModels;

/// <summary>
/// カート内の1商品を表すViewModel。
/// 数量変更時に税込小計を再計算し、変更通知を発行する。
/// </summary>
public partial class CartItemViewModel : ObservableObject
{
    /// <summary>商品ID。</summary>
    public int ProductId { get; init; }

    /// <summary>商品名。</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>税抜単価。</summary>
    public decimal UnitPrice { get; init; }

    /// <summary>適用税率（8 or 10）。</summary>
    public int TaxRate { get; init; }

    /// <summary>数量。最小値は1。</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    [NotifyPropertyChangedFor(nameof(LineTotalWithTax))]
    private int _quantity = 1;

    /// <summary>税抜小計（UnitPrice × Quantity）。</summary>
    public decimal LineTotal => UnitPrice * Quantity;

    /// <summary>税込小計（Floor(LineTotal × (1 + TaxRate / 100))）。</summary>
    public decimal LineTotalWithTax => Math.Floor(LineTotal * (1 + (decimal)TaxRate / 100m));

    /// <summary>
    /// 数量が変更される前のバリデーション。
    /// 1未満の値を設定すると <see cref="ArgumentOutOfRangeException"/> がスローされる。
    /// </summary>
    partial void OnQuantityChanging(int value)
    {
        if (value < 1)
            throw new ArgumentOutOfRangeException(nameof(value), "数量は1以上である必要があります。");
    }
}
