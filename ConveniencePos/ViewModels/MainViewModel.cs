using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConveniencePos.Data;
using ConveniencePos.Models;
using Microsoft.EntityFrameworkCore;

namespace ConveniencePos.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly PosDbContext _dbContext = new();

    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    [ObservableProperty]
    private decimal _receivedAmount;

    public ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    public decimal Subtotal => CartItems.Sum(i => i.LineTotal);

    public decimal TaxableAmount8 => CartItems
        .Where(i => i.TaxRate == 8)
        .Sum(i => i.LineTotal);

    public decimal TaxableAmount10 => CartItems
        .Where(i => i.TaxRate == 10)
        .Sum(i => i.LineTotal);

    public decimal TaxAmount8 => Math.Floor(TaxableAmount8 * 0.08m);

    public decimal TaxAmount10 => Math.Floor(TaxableAmount10 * 0.10m);

    public decimal TaxAmount => TaxAmount8 + TaxAmount10;

    public decimal TotalAmount => Subtotal + TaxAmount;

    public decimal Change => ReceivedAmount > TotalAmount ? ReceivedAmount - TotalAmount : 0;

    [RelayCommand]
    private async Task AddItemAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeInput))
            return;

        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.JanCode == BarcodeInput);

        if (product is null)
            return;

        var existing = CartItems.FirstOrDefault(i => i.ProductId == product.Id);
        if (existing is not null)
        {
            existing.Quantity++;
        }
        else
        {
            var item = new CartItemViewModel
            {
                ProductId = product.Id,
                Name = product.Name,
                UnitPrice = product.Price,
                TaxRate = product.TaxRate,
                Quantity = 1
            };
            item.PropertyChanged += OnCartItemPropertyChanged;
            CartItems.Add(item);
        }

        BarcodeInput = string.Empty;
        RefreshTotals();
    }

    [RelayCommand]
    private async Task ConfirmTransactionAsync()
    {
        if (CartItems.Count == 0)
            return;

        var transaction = new Transaction
        {
            CreatedAt = DateTime.Now,
            TotalAmount = TotalAmount,
            TaxAmount = TaxAmount,
            Items = CartItems.Select(i => new TransactionItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                AppliedTaxRate = i.TaxRate
            }).ToList()
        };

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        var trxId = transaction.Id;
        var received = ReceivedAmount;
        var change = Change;
        var snapshot = CartItems
            .Select(c => new { c.Name, c.Quantity, c.LineTotalWithTax, c.TaxRate })
            .ToList();
        var sub = Subtotal;
        var taxable8 = TaxableAmount8;
        var taxable10 = TaxableAmount10;
        var tax8 = TaxAmount8;
        var tax10 = TaxAmount10;
        var taxTotal = TaxAmount;
        var total = TotalAmount;

        var sb = new StringBuilder();
        const int w = 32;
        sb.AppendLine(new string('=', w));
        sb.AppendLine(CenterText("Convenience POS Store", w));
        sb.AppendLine("レジ#01  担当: 谷本 レジ担当");
        sb.AppendLine();
        sb.AppendLine($"取引番号: TRX-{trxId}");
        sb.AppendLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm"));
        sb.AppendLine();
        foreach (var item in snapshot)
        {
            sb.AppendLine($"{item.Name} {item.Quantity}  ¥{item.LineTotalWithTax:N0} {item.TaxRate}%");
        }
        sb.AppendLine(AmountLine("税抜合計", $"¥{sub:N0}"));
        sb.AppendLine($"8% 対象: ¥{taxable8:N0} 消費税: ¥{tax8:N0}");
        sb.AppendLine($"10%対象: ¥{taxable10:N0} 消費税: ¥{tax10:N0}");
        sb.AppendLine(AmountLine("消費税合計", $"¥{taxTotal:N0}"));
        sb.AppendLine();
        sb.AppendLine(AmountLine("税込合計", $"¥{total:N0}"));
        sb.AppendLine(AmountLine("[現金] お預かり", $"¥{received:N0}"));
        sb.AppendLine(AmountLine("お釣り", $"¥{change:N0}"));
        sb.AppendLine("ありがとうお越し下さいました");

        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        File.WriteAllText(Path.Combine(desktop, $"receipt_{trxId}.txt"), sb.ToString());

        foreach (var item in CartItems)
            item.PropertyChanged -= OnCartItemPropertyChanged;
        CartItems.Clear();
        ReceivedAmount = 0;
        RefreshTotals();
    }

    partial void OnReceivedAmountChanged(decimal value)
    {
        OnPropertyChanged(nameof(Change));
    }

    private void OnCartItemPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CartItemViewModel.Quantity))
            RefreshTotals();
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(TaxableAmount8));
        OnPropertyChanged(nameof(TaxableAmount10));
        OnPropertyChanged(nameof(TaxAmount8));
        OnPropertyChanged(nameof(TaxAmount10));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(Change));
    }

    private static int DisplayWidth(string s)
    {
        int width = 0;
        foreach (var c in s)
        {
            if (c >= 0x3000 && c < 0xA000) width += 2;
            else if (c >= 0xAC00 && c < 0xD800) width += 2;
            else if (c >= 0xF900 && c < 0xFB00) width += 2;
            else if (c >= 0xFF01 && c < 0xFF5F) width += 2;
            else width += 1;
        }
        return width;
    }

    private static string CenterText(string text, int totalWidth)
    {
        int pad = Math.Max(0, (totalWidth - DisplayWidth(text)) / 2);
        return new string(' ', pad) + text;
    }

    private static string AmountLine(string label, string amount)
    {
        const int width = 32;
        int spaces = width - DisplayWidth(label) - DisplayWidth(amount);
        return label + new string(' ', Math.Max(1, spaces)) + amount;
    }
}

public partial class CartItemViewModel : ObservableObject
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    public int TaxRate { get; init; }

    [ObservableProperty]
    private int _quantity;

    public decimal LineTotal => UnitPrice * Quantity;

    public decimal LineTotalWithTax => Math.Floor(LineTotal * (1 + TaxRate / 100m));

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(LineTotal));
        OnPropertyChanged(nameof(LineTotalWithTax));
    }
}
