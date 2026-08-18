using System.IO;
using ConveniencePos.Data;
using ConveniencePos.Data.Seed;
using ConveniencePos.Models;
using ConveniencePos.Services;
using ConveniencePos.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConveniencePos.Tests.Integration;

public class TransactionIntegrationTests : IDisposable
{
    private readonly IDbContextFactory<PosDbContext> _contextFactory;
    private readonly BarcodeService _barcodeService;
    private readonly TransactionService _transactionService;
    private readonly ReceiptService _receiptService;
    private readonly string _tempDir;

    public TransactionIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var factoryMock = new Mock<IDbContextFactory<PosDbContext>>();
        factoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new PosDbContext(options));

        _contextFactory = factoryMock.Object;

        using var db = new PosDbContext(options);
        db.Database.EnsureCreated();

        _barcodeService = new BarcodeService(_contextFactory, Mock.Of<ILogger<BarcodeService>>());
        _transactionService = new TransactionService(_contextFactory, Mock.Of<ILogger<TransactionService>>());

        _tempDir = Path.Combine(Path.GetTempPath(), "ConveniencePosTests_" + Guid.NewGuid().ToString("N"));
        _receiptService = new ReceiptService(
            outputDirectory: _tempDir,
            logger: Mock.Of<ILogger<ReceiptService>>());
    }

    [Fact]
    public async Task FullFlow_ScanAndCheckout()
    {
        var product = await _barcodeService.LookupByBarcodeAsync("777777");
        Assert.NotNull(product);

        var items = new List<TransactionItem>
        {
            new()
            {
                ProductId = product.Id,
                Quantity = 2,
                UnitPrice = product.Price,
                AppliedTaxRate = product.TaxRate
            }
        };

        var transaction = await _transactionService.SaveTransactionAsync(259m, 19m, items);
        Assert.True(transaction.Id > 0);

        var receiptContext = new ReceiptContext(
            transaction.Id,
            transaction.CreatedAt,
            [new ReceiptItem(product.Name, 2, 259m, 8)],
            240m, 240m, 0m, 19m, 0m, 19m, 259m, 300m, 41m);

        var receiptText = _receiptService.GenerateReceipt(receiptContext);
        Assert.Contains("TRX-", receiptText);
        Assert.Contains("おにぎり 梅", receiptText);

        await _receiptService.SaveReceiptAsync(transaction.Id, receiptText);
        var filePath = Path.Combine(_tempDir, $"receipt_{transaction.Id}.txt");
        Assert.True(File.Exists(filePath));

        var savedReceipt = await File.ReadAllTextAsync(filePath);
        Assert.Contains("おにぎり 梅", savedReceipt);
    }

    [Fact]
    public async Task FullFlow_MixedTaxItems()
    {
        var product8 = await _barcodeService.LookupByBarcodeAsync("777777");
        var product10 = await _barcodeService.LookupByBarcodeAsync("999999");
        Assert.NotNull(product8);
        Assert.NotNull(product10);

        var items = new List<TransactionItem>
        {
            new() { ProductId = product8.Id, Quantity = 1, UnitPrice = product8.Price, AppliedTaxRate = product8.TaxRate },
            new() { ProductId = product10.Id, Quantity = 1, UnitPrice = product10.Price, AppliedTaxRate = product10.TaxRate }
        };

        var transaction = await _transactionService.SaveTransactionAsync(327m, 27m, items);
        Assert.NotNull(transaction);
        Assert.Equal(327m, transaction.TotalAmount);
    }

    [Fact]
    public async Task FullFlow_ReceiptContainsAllFields()
    {
        var product = await _barcodeService.LookupByBarcodeAsync("999999");
        Assert.NotNull(product);

        var items = new List<TransactionItem>
        {
            new() { ProductId = product.Id, Quantity = 1, UnitPrice = product.Price, AppliedTaxRate = product.TaxRate }
        };

        var transaction = await _transactionService.SaveTransactionAsync(198m, 18m, items);

        var receiptContext = new ReceiptContext(
            transaction.Id,
            transaction.CreatedAt,
            [new ReceiptItem(product.Name, 1, 198m, 10)],
            180m, 0m, 180m, 0m, 18m, 18m, 198m, 200m, 2m);

        var receiptText = _receiptService.GenerateReceipt(receiptContext);

        Assert.Contains("Convenience POS Store", receiptText);
        Assert.Contains("レジ#01", receiptText);
        Assert.Contains("谷本 レジ担当", receiptText);
        Assert.Contains("TRX-", receiptText);
        Assert.Contains("ポテトチップス", receiptText);
        Assert.Contains("お預かり", receiptText);
        Assert.Contains("お釣り", receiptText);
    }

    [Fact]
    public async Task BarcodeSearch_AllSeedProducts_AreFound()
    {
        var barcodes = new[] { "777777", "888888", "999999", "111111", "222222" };
        foreach (var barcode in barcodes)
        {
            var product = await _barcodeService.LookupByBarcodeAsync(barcode);
            Assert.NotNull(product);
        }
    }

    [Fact]
    public async Task TransactionService_SavesCorrectItemDetails()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 3, UnitPrice = 120m, AppliedTaxRate = 8 },
            new() { ProductId = 3, Quantity = 2, UnitPrice = 180m, AppliedTaxRate = 10 }
        };

        var transaction = await _transactionService.SaveTransactionAsync(681m, 54m, items);

        await using var db = await _contextFactory.CreateDbContextAsync();
        var loaded = await db.Transactions
            .Include(t => t.Items)
            .FirstAsync(t => t.Id == transaction.Id);

        Assert.Equal(2, loaded.Items.Count);
        Assert.Contains(loaded.Items, i => i.ProductId == 1 && i.Quantity == 3 && i.AppliedTaxRate == 8);
        Assert.Contains(loaded.Items, i => i.ProductId == 3 && i.Quantity == 2 && i.AppliedTaxRate == 10);
    }

    [Fact]
    public async Task ViewModel_Integration_WithRealServices()
    {
        var vm = new MainViewModel(_barcodeService, _transactionService, _receiptService,
            Mock.Of<ILogger<MainViewModel>>());

        vm.BarcodeInput = "777777";
        await vm.AddItemCommand.ExecuteAsync(null);

        Assert.Single(vm.CartItems);
        Assert.Equal(120m, vm.Subtotal);
        Assert.Equal(129m, vm.TotalAmount);
    }

    [Fact]
    public async Task ViewModel_Integration_DuplicateBarcode()
    {
        var vm = new MainViewModel(_barcodeService, _transactionService, _receiptService,
            Mock.Of<ILogger<MainViewModel>>());

        vm.BarcodeInput = "777777";
        await vm.AddItemCommand.ExecuteAsync(null);
        vm.BarcodeInput = "777777";
        await vm.AddItemCommand.ExecuteAsync(null);

        Assert.Single(vm.CartItems);
        Assert.Equal(2, vm.CartItems[0].Quantity);
    }

    [Fact]
    public async Task ViewModel_Integration_CheckoutAndReset()
    {
        var vm = new MainViewModel(_barcodeService, _transactionService, _receiptService,
            Mock.Of<ILogger<MainViewModel>>());

        vm.BarcodeInput = "777777";
        await vm.AddItemCommand.ExecuteAsync(null);
        vm.ReceivedAmount = 200m;

        await vm.ConfirmTransactionCommand.ExecuteAsync(null);

        Assert.Empty(vm.CartItems);
        Assert.Equal(0m, vm.ReceivedAmount);
        Assert.Equal(0m, vm.TotalAmount);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, true);
    }
}
