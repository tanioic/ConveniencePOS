using System.Collections.ObjectModel;
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

    public decimal TaxAmount => Math.Floor(Subtotal * 0.1m);

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
            CartItems.Add(new CartItemViewModel
            {
                ProductId = product.Id,
                Name = product.Name,
                UnitPrice = product.Price,
                Quantity = 1
            });
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
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        CartItems.Clear();
        ReceivedAmount = 0;
        RefreshTotals();
    }

    partial void OnReceivedAmountChanged(decimal value)
    {
        OnPropertyChanged(nameof(Change));
    }

    private void RefreshTotals()
    {
        OnPropertyChanged(nameof(Subtotal));
        OnPropertyChanged(nameof(TaxAmount));
        OnPropertyChanged(nameof(TotalAmount));
        OnPropertyChanged(nameof(Change));
    }
}

public partial class CartItemViewModel : ObservableObject
{
    public int ProductId { get; init; }

    public string Name { get; init; } = string.Empty;

    public decimal UnitPrice { get; init; }

    [ObservableProperty]
    private int _quantity;

    public decimal LineTotal => UnitPrice * Quantity;

    partial void OnQuantityChanged(int value)
    {
        OnPropertyChanged(nameof(LineTotal));
    }
}
