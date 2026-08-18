using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Common;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ConveniencePos.Models;
using ConveniencePos.Services;
using Microsoft.Extensions.Logging;

namespace ConveniencePos.ViewModels;

/// <summary>
/// メイン画面のViewModel。カート操作、合計計算、取引確定を管理する。
/// </summary>
public partial class MainViewModel : ObservableObject, IDisposable
{
    private readonly IBarcodeService _barcodeService;
    private readonly ITransactionService _transactionService;
    private readonly IReceiptService _receiptService;
    private readonly ILogger<MainViewModel> _logger;
    private bool _disposed;

    /// <summary>
    /// コンストラクタ。DIコンテナからサービスとロガーを受け取る。
    /// </summary>
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

    /// <summary>バーコード入力値。</summary>
    [ObservableProperty]
    private string _barcodeInput = string.Empty;

    /// <summary>預かり金額。</summary>
    [ObservableProperty]
    private decimal _receivedAmount;

    /// <summary>エラーメッセージ。</summary>
    [ObservableProperty]
    private string _errorMessage = string.Empty;

    /// <summary>エラーが発生しているかどうか。</summary>
    [ObservableProperty]
    private bool _hasError;

    /// <summary>カート内の商品リスト。</summary>
    public ObservableCollection<CartItemViewModel> CartItems { get; } = new();

    /// <summary>税抜合計金額。</summary>
    public decimal Subtotal => CartItems.Sum(i => i.LineTotal);

    /// <summary>8%軽減税率対象の合計金額。</summary>
    public decimal TaxableAmount8 => CartItems
        .Where(i => i.TaxRate == 8)
        .Sum(i => i.LineTotal);

    /// <summary>10%標準税率対象の合計金額。</summary>
    public decimal TaxableAmount10 => CartItems
        .Where(i => i.TaxRate == 10)
        .Sum(i => i.LineTotal);

    /// <summary>8%軽減税率の消費税額（端数切捨て）。</summary>
    public decimal TaxAmount8 => Math.Floor(TaxableAmount8 * 0.08m);

    /// <summary>10%標準税率の消費税額（端数切捨て）。</summary>
    public decimal TaxAmount10 => Math.Floor(TaxableAmount10 * 0.10m);

    /// <summary>消費税合計額。</summary>
    public decimal TaxAmount => TaxAmount8 + TaxAmount10;

    /// <summary>税込合計金額。</summary>
    public decimal TotalAmount => Subtotal + TaxAmount;

    /// <summary>会計確定が可能かどうか（カートに商品があり、預かり金額が十分）。</summary>
    public bool CanConfirmTransaction => CartItems.Count > 0 && ReceivedAmount >= TotalAmount;

    /// <summary>お釣り金額。</summary>
    public decimal Change => ReceivedAmount > TotalAmount ? ReceivedAmount - TotalAmount : 0;

    /// <summary>
    /// バーコードをスキャンしてカートに商品を追加する。
    /// </summary>
    [RelayCommand]
    internal async Task AddItemAsync(CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(BarcodeInput))
            return;

        ClearError();

        try
        {
            var product = await _barcodeService.LookupByBarcodeAsync(BarcodeInput, cancellationToken);

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
        catch (OperationCanceledException)
        {
            _logger.LogWarning("商品検索がキャンセルされました (JAN: {Barcode})", BarcodeInput);
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "データベース接続エラーが発生しました (JAN: {Barcode})", BarcodeInput);
            SetError("データベースに接続できません。接続設定を確認してください。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "商品検索中に予期せぬエラーが発生しました (JAN: {Barcode})", BarcodeInput);
            SetError("商品検索中にエラーが発生しました。管理者に連絡してください。");
        }
    }

    /// <summary>
    /// 会計を確定し、取引を保存してレシートを出力する。
    /// </summary>
    [RelayCommand]
    internal async Task ConfirmTransactionAsync(CancellationToken cancellationToken = default)
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
            var items = CartItems.Select(i => new TransactionItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                AppliedTaxRate = i.TaxRate
            }).ToList();

            var transaction = await _transactionService.SaveTransactionAsync(
                TotalAmount, TaxAmount, items, cancellationToken);

            _logger.LogInformation("取引 TRX-{TransactionId} を保存しました (合計: {TotalAmount})", transaction.Id, TotalAmount);

            try
            {
                var context = new ReceiptContext(
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

                var receiptContent = _receiptService.GenerateReceipt(context);
                await _receiptService.SaveReceiptAsync(transaction.Id, receiptContent, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("レシート出力がキャンセルされました (TRX-{TransactionId})", transaction.Id);
            }
            catch (IOException ex)
            {
                _logger.LogWarning(ex, "レシートファイルの書き込みに失敗しました (TRX-{TransactionId})", transaction.Id);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "レシートファイルの書き込み権限がありません (TRX-{TransactionId})", transaction.Id);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "レシート出力中に予期せぬエラーが発生しました (TRX-{TransactionId})", transaction.Id);
            }

            foreach (var item in CartItems)
                item.PropertyChanged -= OnCartItemPropertyChanged;
            CartItems.Clear();
            ReceivedAmount = 0;
            RefreshTotals();
            OnPropertyChanged(nameof(CanConfirmTransaction));
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("取引保存がキャンセルされました");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "取引保存に失敗しました");
            SetError(ex.Message);
        }
        catch (DbException ex)
        {
            _logger.LogError(ex, "データベースエラーが発生しました");
            SetError("データベースに接続できません。接続設定を確認してください。");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "取引確定中に予期せぬエラーが発生しました");
            SetError("取引の確定に失敗しました。管理者に連絡してください。");
        }
    }

    /// <inheritdoc/>
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

    /// <inheritdoc/>
    public void Dispose()
    {
        if (!_disposed)
        {
            foreach (var item in CartItems)
                item.PropertyChanged -= OnCartItemPropertyChanged;
            _disposed = true;
        }
        GC.SuppressFinalize(this);
    }
}
