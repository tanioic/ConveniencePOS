global using ConveniencePos.Models;
using ConveniencePos.Data;
using ConveniencePos.Data.Seed;
using ConveniencePos.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConveniencePos.Tests.Services;

public class BarcodeServiceTests : IDisposable
{
    private readonly IDbContextFactory<PosDbContext> _contextFactory;
    private readonly BarcodeService _barcodeService;

    public BarcodeServiceTests()
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var factoryMock = new Mock<IDbContextFactory<PosDbContext>>();
        factoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new PosDbContext(options));

        _contextFactory = factoryMock.Object;

        var loggerMock = new Mock<ILogger<BarcodeService>>();
        _barcodeService = new BarcodeService(_contextFactory, loggerMock.Object);

        using var db = new PosDbContext(options);
        db.Database.EnsureCreated();
    }

    [Fact]
    public async Task LookupByBarcode_ExistingProduct_ReturnsProduct()
    {
        var result = await _barcodeService.LookupByBarcodeAsync("777777");
        Assert.NotNull(result);
        Assert.Equal("おにぎり 梅", result.Name);
    }

    [Fact]
    public async Task LookupByBarcode_ExistingProduct_ReturnsCorrectPrice()
    {
        var result = await _barcodeService.LookupByBarcodeAsync("999999");
        Assert.NotNull(result);
        Assert.Equal(180m, result.Price);
    }

    [Fact]
    public async Task LookupByBarcode_ExistingProduct_ReturnsCorrectTaxRate()
    {
        var result = await _barcodeService.LookupByBarcodeAsync("777777");
        Assert.NotNull(result);
        Assert.Equal(8, result.TaxRate);
    }

    [Fact]
    public async Task LookupByBarcode_NonExisting_ReturnsNull()
    {
        var result = await _barcodeService.LookupByBarcodeAsync("000000");
        Assert.Null(result);
    }

    [Fact]
    public async Task LookupByBarcode_EmptyString_ReturnsNull()
    {
        var result = await _barcodeService.LookupByBarcodeAsync("");
        Assert.Null(result);
    }

    [Fact]
    public async Task LookupByBarcode_AllSeedProducts_Found()
    {
        var barcodes = new[] { "777777", "888888", "999999", "111111", "222222" };
        foreach (var barcode in barcodes)
        {
            var result = await _barcodeService.LookupByBarcodeAsync(barcode);
            Assert.NotNull(result);
        }
    }

    [Fact]
    public async Task LookupByBarcode_Tissue_ProductFound()
    {
        var result = await _barcodeService.LookupByBarcodeAsync("111111");
        Assert.NotNull(result);
        Assert.Equal("ティッシュ", result.Name);
        Assert.Equal(200m, result.Price);
        Assert.Equal(10, result.TaxRate);
    }

    [Fact]
    public async Task LookupByBarcode_GreenTea_ProductFound()
    {
        var result = await _barcodeService.LookupByBarcodeAsync("888888");
        Assert.NotNull(result);
        Assert.Equal("緑茶 500ml", result.Name);
        Assert.Equal(150m, result.Price);
        Assert.Equal(8, result.TaxRate);
    }

    [Fact]
    public async Task LookupByBarcode_Coffee_ProductFound()
    {
        var result = await _barcodeService.LookupByBarcodeAsync("222222");
        Assert.NotNull(result);
        Assert.Equal("コーヒー 熱 350ml", result.Name);
        Assert.Equal(110m, result.Price);
        Assert.Equal(10, result.TaxRate);
    }

    [Fact]
    public async Task LookupByBarcode_NullBarcode_ThrowsException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _barcodeService.LookupByBarcodeAsync(null!));
    }

    public void Dispose() { }
}
