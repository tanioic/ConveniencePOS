using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConveniencePos.Models;
using ConveniencePos.Services;
using Microsoft.Extensions.Logging;

namespace ConveniencePos.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IBarcodeService _barcodeService;
    private readonly ITransactionService _transactionService;
    private readonly IReceiptService _receiptService;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Change))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmTransactionCommand))]
    private decimal _receivedAmount;

    [ObservableProperty]
    private decimal _subtotal;

    [ObservableProperty]
    private decimal _taxableAmount8;

    [ObservableProperty]
    private decimal _taxableAmount10;

    [ObservableProperty]
    private decimal _taxAmount8;

    [ObservableProperty]
    private decimal _taxAmount10;

    [ObservableProperty]
    private decimal _taxAmount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Change))]
    [NotifyCanExecuteChangedFor(nameof(ConfirmTransactionCommand))]
    private decimal _totalAmount;

    public decimal Change => Math.Max(0, ReceivedAmount - TotalAmount);

    public ObservableCollection<CartItemViewModel> CartItems { get; } = [];

    public MainViewModel(
        IBarcodeService barcodeService,
        ITransactionService transactionService,
        IReceiptService receiptService,
        ILogger<MainViewModel> logger)
    {
        _barcodeService = barcodeService;
        _transactionService = transactionService;
        _receiptService = receiptService;
        _logger = logger;
    }

    [RelayCommand]
    private async Task AddItemAsync()
    {
        if (string.IsNullOrWhiteSpace(BarcodeInput))
            return;

        try
        {
            var product = await _barcodeService.LookupByBarcodeAsync(BarcodeInput);

            if (product is null)
            {
                _logger.LogWarning("商品が見つかりません (JAN: {Barcode})", BarcodeInput);
                BarcodeInput = string.Empty;
                return;
            }

            var existing = CartItems.FirstOrDefault(c => c.ProductId == product.Id);
            if (existing is not null)
            {
                existing.Quantity++;
            }
            else
            {
                var item = new CartItemViewModel(product.Id, product.Name, product.Price, product.TaxRate);
                item.PropertyChanged += (_, _) => RefreshTotals();
                CartItems.Add(item);
            }

            RefreshTotals();
            BarcodeInput = string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品検索中にエラーが発生しました");
        }
    }

    private bool CanConfirmTransaction()
    {
        return CartItems.Count > 0 && ReceivedAmount >= TotalAmount;
    }

    [RelayCommand(CanExecute = nameof(CanConfirmTransaction))]
    private async Task ConfirmTransactionAsync()
    {
        try
        {
            var items = CartItems.Select(c => new TransactionItem
            {
                ProductId = c.ProductId,
                Quantity = c.Quantity,
                UnitPrice = c.UnitPrice,
                AppliedTaxRate = c.TaxRate
            }).ToList();

            var transaction = await _transactionService.SaveTransactionAsync(TotalAmount, TaxAmount, items);

            var receiptContext = new ReceiptContext(
                transaction.Id,
                transaction.CreatedAt,
                CartItems.Select(c => new ReceiptItem(c.Name, c.Quantity, c.LineTotalWithTax, c.TaxRate)).ToList(),
                Subtotal,
                TaxableAmount8,
                TaxableAmount10,
                TaxAmount8,
                TaxAmount10,
                TaxAmount,
                TotalAmount,
                ReceivedAmount,
                Change);

            var receiptText = _receiptService.GenerateReceipt(receiptContext);

            try
            {
                await _receiptService.SaveReceiptAsync(transaction.Id, receiptText);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "レシート出力に失敗しました");
            }

            CartItems.Clear();
            ReceivedAmount = 0;
            RefreshTotals();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "会計確定処理中にエラーが発生しました");
        }
    }

    public void RefreshTotals()
    {
        Subtotal = CartItems.Sum(c => c.LineTotal);
        TaxableAmount8 = CartItems.Where(c => c.TaxRate == 8).Sum(c => c.LineTotal);
        TaxableAmount10 = CartItems.Where(c => c.TaxRate == 10).Sum(c => c.LineTotal);
        TaxAmount8 = (int)Math.Floor(TaxableAmount8 * 0.08m);
        TaxAmount10 = (int)Math.Floor(TaxableAmount10 * 0.10m);
        TaxAmount = TaxAmount8 + TaxAmount10;
        TotalAmount = Subtotal + TaxAmount;
    }
}
