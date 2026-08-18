using System.IO;
using ConveniencePos.Data;
using ConveniencePos.Models;
using ConveniencePos.Services;
using ConveniencePos.ViewModels;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace ConveniencePos.Tests.Integration;

/// <summary>
/// S-009: 会計確定→DB保存→レシート出力 の結合テスト。
/// InMemory DB + 実際の ReceiptService を使用して、
/// ConfirmTransactionAsync の全フローを検証する。
/// </summary>
public class TransactionIntegrationTests : IDisposable
{
    private readonly PosDbContext _dbContext;
    private readonly MainViewModel _sut;
    private readonly string _tempReceiptDir;

    public TransactionIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _dbContext = new PosDbContext(options);
        SeedProducts();

        var barcodeService = new BarcodeService(_dbContext);

        _tempReceiptDir = Path.Combine(Path.GetTempPath(), $"pos_uat_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempReceiptDir);

        var receiptService = new ReceiptService(
            storeName: "テスト店舗",
            registerNumber: "レジ#99",
            operatorName: "テスト担当",
            outputDirectory: _tempReceiptDir,
            width: 32);

        _sut = new MainViewModel(_dbContext, barcodeService, receiptService);
    }

    private void SeedProducts()
    {
        _dbContext.Products.AddRange(
            new Product { Id = 1, JanCode = "777777", Name = "おにぎり 梅", Price = 120m, TaxRate = 8 },
            new Product { Id = 2, JanCode = "888888", Name = "緑茶 500ml", Price = 150m, TaxRate = 8 },
            new Product { Id = 3, JanCode = "999999", Name = "ポテトチップス", Price = 180m, TaxRate = 10 },
            new Product { Id = 4, JanCode = "111111", Name = "ティッシュ", Price = 200m, TaxRate = 10 },
            new Product { Id = 5, JanCode = "222222", Name = "コーヒー 熱 350ml", Price = 110m, TaxRate = 10 }
        );
        _dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _sut.Dispose();
        if (Directory.Exists(_tempReceiptDir))
            Directory.Delete(_tempReceiptDir, true);
    }

    private async Task ScanBarcodeAsync(string janCode)
    {
        _sut.BarcodeInput = janCode;
        await _sut.AddItemAsync();
    }

    [Fact]
    public async Task S009_ConfirmTransaction_SavesToDb()
    {
        await ScanBarcodeAsync("777777");

        Assert.Single(_sut.CartItems);
        Assert.Equal("おにぎり 梅", _sut.CartItems[0].Name);
        Assert.Equal(120m, _sut.CartItems[0].UnitPrice);
        Assert.Equal(8, _sut.CartItems[0].TaxRate);
        Assert.Equal(129m, _sut.TotalAmount);

        _sut.ReceivedAmount = 200m;
        Assert.True(_sut.CanConfirmTransaction);

        await _sut.ConfirmTransactionAsync();

        var transactions = await _dbContext.Transactions.ToListAsync();
        Assert.Single(transactions);
        Assert.Equal(129m, transactions[0].TotalAmount);
        Assert.Equal(9m, transactions[0].TaxAmount);

        var items = await _dbContext.TransactionItems.ToListAsync();
        Assert.Single(items);
        Assert.Equal(1, items[0].ProductId);
        Assert.Equal(1, items[0].Quantity);
        Assert.Equal(120m, items[0].UnitPrice);
        Assert.Equal(8, items[0].AppliedTaxRate);
    }

    [Fact]
    public async Task S009_ConfirmTransaction_ClearsCart()
    {
        await ScanBarcodeAsync("777777");
        await ScanBarcodeAsync("999999");

        Assert.Equal(2, _sut.CartItems.Count);

        _sut.ReceivedAmount = 500m;
        await _sut.ConfirmTransactionAsync();

        Assert.Empty(_sut.CartItems);
        Assert.Equal(0m, _sut.ReceivedAmount);
        Assert.Equal(0m, _sut.Subtotal);
        Assert.Equal(0m, _sut.TotalAmount);
        Assert.False(_sut.CanConfirmTransaction);
    }

    [Fact]
    public async Task S009_ConfirmTransaction_GeneratesReceiptFile()
    {
        await ScanBarcodeAsync("777777");
        await ScanBarcodeAsync("222222");

        _sut.ReceivedAmount = 500m;
        await _sut.ConfirmTransactionAsync();

        var transactions = await _dbContext.Transactions.ToListAsync();
        Assert.Single(transactions);
        var trxId = transactions[0].Id;

        var receiptFile = Path.Combine(_tempReceiptDir, $"receipt_{trxId}.txt");
        Assert.True(File.Exists(receiptFile), $"レシートファイルが見つかりません: {receiptFile}");

        var receiptContent = await File.ReadAllTextAsync(receiptFile);
        Assert.Contains("テスト店舗", receiptContent);
        Assert.Contains("レジ#99", receiptContent);
        Assert.Contains($"TRX-{trxId}", receiptContent);
        Assert.Contains("おにぎり 梅", receiptContent);
        Assert.Contains("コーヒー", receiptContent);
        Assert.Contains("お預かり", receiptContent);
        Assert.Contains("お釣り", receiptContent);
        Assert.Contains("税込合計", receiptContent);
    }

    [Fact]
    public async Task S009_ConfirmTransaction_ReceiptContent_CalculatesCorrectly()
    {
        await ScanBarcodeAsync("777777");
        await ScanBarcodeAsync("999999");

        _sut.ReceivedAmount = 400m;
        await _sut.ConfirmTransactionAsync();

        var transactions = await _dbContext.Transactions.ToListAsync();
        Assert.Single(transactions);
        var receiptFile = Path.Combine(_tempReceiptDir, $"receipt_{transactions[0].Id}.txt");
        var content = await File.ReadAllTextAsync(receiptFile);

        Assert.Contains("¥327", content);
        Assert.Contains("¥400", content);
        Assert.Contains("¥73", content);
        Assert.Contains("8%", content);
        Assert.Contains("10%", content);
    }

    [Fact]
    public async Task S009_ConfirmTransaction_CorrectChange()
    {
        await ScanBarcodeAsync("777777");

        _sut.ReceivedAmount = 129m;
        await _sut.ConfirmTransactionAsync();

        Assert.Equal(0m, _sut.Change);
        Assert.Empty(_sut.CartItems);
    }

    [Fact]
    public async Task S009_ConfirmTransaction_MultipleItems_AllSaved()
    {
        var codes = new[] { "777777", "888888", "999999", "111111", "222222" };
        foreach (var code in codes)
            await ScanBarcodeAsync(code);

        Assert.Equal(5, _sut.CartItems.Count);
        _sut.ReceivedAmount = 1000m;
        await _sut.ConfirmTransactionAsync();

        var items = await _dbContext.TransactionItems.ToListAsync();
        Assert.Equal(5, items.Count);
        Assert.Empty(_sut.CartItems);
    }

    [Fact]
    public async Task S009_ConfirmTransaction_TaxBreakdown_Correct()
    {
        await ScanBarcodeAsync("777777");
        await ScanBarcodeAsync("999999");

        _sut.ReceivedAmount = 400m;
        await _sut.ConfirmTransactionAsync();

        var trx = await _dbContext.Transactions.FirstAsync();
        Assert.Equal(27m, trx.TaxAmount);
        Assert.Equal(327m, trx.TotalAmount);
    }

    [Fact]
    public async Task S009_SecondTransaction_AutoIncrementId()
    {
        await ScanBarcodeAsync("777777");
        _sut.ReceivedAmount = 200m;
        await _sut.ConfirmTransactionAsync();

        await ScanBarcodeAsync("999999");
        _sut.ReceivedAmount = 200m;
        await _sut.ConfirmTransactionAsync();

        var transactions = await _dbContext.Transactions.ToListAsync();
        Assert.Equal(2, transactions.Count);
        Assert.NotEqual(transactions[0].Id, transactions[1].Id);
    }

    [Fact]
    public async Task S009_ReceiptFile_CreatedOnDesktop_WithRealReceiptService()
    {
        var realReceiptService = new ReceiptService(
            storeName: "Convenience POS Store",
            registerNumber: "レジ#01",
            operatorName: "谷本 レジ担当",
            outputDirectory: "Desktop",
            width: 32);

        using var dbContext2 = new PosDbContext(
            new DbContextOptionsBuilder<PosDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options);

        dbContext2.Products.AddRange(
            new Product { Id = 1, JanCode = "777777", Name = "おにぎり 梅", Price = 120m, TaxRate = 8 }
        );
        dbContext2.SaveChanges();

        var vm = new MainViewModel(dbContext2, new BarcodeService(dbContext2), realReceiptService);

        vm.BarcodeInput = "777777";
        await vm.AddItemAsync();

        vm.ReceivedAmount = 200m;
        await vm.ConfirmTransactionAsync();

        var trx = await dbContext2.Transactions.FirstAsync();
        var desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var receiptFile = Path.Combine(desktopPath, $"receipt_{trx.Id}.txt");

        Assert.True(File.Exists(receiptFile), $"デスクトップにレシートファイルが見つかりません: {receiptFile}");

        var content = await File.ReadAllTextAsync(receiptFile);
        Assert.Contains("Convenience POS Store", content);
        Assert.Contains("おにぎり 梅", content);
        Assert.Contains("税込合計", content);

        File.Delete(receiptFile);
    }
}
