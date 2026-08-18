using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConveniencePos.Data;
using ConveniencePos.Models;
using ConveniencePos.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ConveniencePos.ViewModels;

public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly PosDbContext _dbContext;
    private readonly IBarcodeService _barcodeService;
    private readonly IReceiptService _receiptService;
    private readonly ILogger<MainViewModel>? _logger;
    private bool _disposed;

    public MainViewModel(
        PosDbContext dbContext,
        IBarcodeService barcodeService,
        IReceiptService receiptService,
        ILogger<MainViewModel>? logger = null)
    {
        _dbContext = dbContext;
        _barcodeService = barcodeService;
        _receiptService = receiptService;
        _logger = logger;
    }

    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    [ObservableProperty]
    private decimal _receivedAmount;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _hasError;

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

    public bool CanConfirmTransaction => CartItems.Count > 0 && ReceivedAmount >= TotalAmount;

    public decimal Change => ReceivedAmount > TotalAmount ? ReceivedAmount - TotalAmount : 0;

    [RelayCommand]
    internal async Task AddItemAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeInput))
            return;

        ClearError();

        try
        {
            var product = await _barcodeService.LookupByBarcodeAsync(BarcodeInput);

            if (product is null)
            {
                SetError($"商品が見つかりません (JAN: {BarcodeInput})");
                return;
            }

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
            OnPropertyChanged(nameof(CanConfirmTransaction));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "商品検索中にエラーが発生しました (JAN: {Barcode})", BarcodeInput);
            SetError("商品検索中にエラーが発生しました。再試行してください。");
        }
    }

    [RelayCommand]
    internal async Task ConfirmTransactionAsync()
    {
        if (CartItems.Count == 0)
            return;

        if (ReceivedAmount < TotalAmount)
        {
            SetError("預かり金額が不足しています。");
            return;
        }

        ClearError();

        try
        {
            var transaction = new Transaction
            {
                CreatedAt = DateTime.UtcNow,
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

            _logger?.LogInformation("取引 TRX-{TransactionId} を保存しました (合計: {TotalAmount})", transaction.Id, TotalAmount);

            try
            {
                var snapshot = CartItems
                    .Select(c => new ReceiptItem(c.Name, c.Quantity, c.LineTotalWithTax, c.TaxRate))
                    .ToList();

                var receiptContent = _receiptService.GenerateReceipt(
                    transaction.Id,
                    transaction.CreatedAt,
                    snapshot,
                    Subtotal,
                    TaxableAmount8,
                    TaxableAmount10,
                    TaxAmount8,
                    TaxAmount10,
                    TaxAmount,
                    TotalAmount,
                    ReceivedAmount,
                    Change);

                _receiptService.SaveReceipt(transaction.Id, receiptContent);
            }
            catch (Exception ex)
            {
                _logger?.LogWarning(ex, "レシート出力に失敗しましたが、取引は保存済みです (TRX-{TransactionId})", transaction.Id);
            }

            foreach (var item in CartItems)
                item.PropertyChanged -= OnCartItemPropertyChanged;
            CartItems.Clear();
            ReceivedAmount = 0;
            RefreshTotals();
            OnPropertyChanged(nameof(CanConfirmTransaction));
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "取引保存中にエラーが発生しました");
            SetError("取引の保存に失敗しました。再試行してください。");
        }
    }

    partial void OnReceivedAmountChanged(decimal value)
    {
        OnPropertyChanged(nameof(Change));
        OnPropertyChanged(nameof(CanConfirmTransaction));
    }

    private void OnCartItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CartItemViewModel.Quantity))
        {
            RefreshTotals();
            OnPropertyChanged(nameof(CanConfirmTransaction));
        }
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

    private void SetError(string message)
    {
        ErrorMessage = message;
        HasError = true;
    }

    private void ClearError()
    {
        ErrorMessage = string.Empty;
        HasError = false;
    }

    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var item in CartItems)
                item.PropertyChanged -= OnCartItemPropertyChanged;
            _dbContext.Dispose();
            _disposed = true;
        }
        GC.SuppressFinalize(this);
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
