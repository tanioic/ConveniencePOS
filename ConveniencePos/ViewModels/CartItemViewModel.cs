using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ConveniencePos.ViewModels;

public partial class CartItemViewModel : ObservableObject
{
    [ObservableProperty]
    private int _productId;

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private decimal _unitPrice;

    [ObservableProperty]
    private int _taxRate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LineTotal))]
    [NotifyPropertyChangedFor(nameof(LineTotalWithTax))]
    private int _quantity = 1;

    public decimal LineTotal => UnitPrice * Quantity;

    public decimal LineTotalWithTax => Math.Floor(LineTotal * (1m + TaxRate / 100m));

    public CartItemViewModel(int productId, string name, decimal unitPrice, int taxRate, int quantity = 1)
    {
        _productId = productId;
        _name = name;
        _unitPrice = unitPrice;
        _taxRate = taxRate;
        _quantity = quantity;
    }

    partial void OnQuantityChanged(int value)
    {
        if (value < 1)
            throw new ArgumentOutOfRangeException(nameof(Quantity), "数量は1以上である必要があります。");
    }
}
