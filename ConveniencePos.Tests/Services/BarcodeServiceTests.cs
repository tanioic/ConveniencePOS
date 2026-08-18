using ConveniencePos.Data;
using ConveniencePos.Models;
using ConveniencePos.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ConveniencePos.Tests.Services;

public class BarcodeServiceTests : IDisposable
{
    private readonly DbContextOptions<PosDbContext> _options;
    private readonly TestDbContextFactory _factory;
    private readonly BarcodeService _sut;

    public BarcodeServiceTests()
    {
        _options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _factory = new TestDbContextFactory(_options);
        SeedTestData();
        _sut = new BarcodeService(_factory, NullLogger<BarcodeService>.Instance);
    }

    private void SeedTestData()
    {
        using var dbContext = new PosDbContext(_options);
        dbContext.Products.AddRange(
            new Product { Id = 1, JanCode = "777777", Name = "おにぎり 梅", Price = 120m, TaxRate = 8 },
            new Product { Id = 2, JanCode = "888888", Name = "緑茶 500ml", Price = 150m, TaxRate = 8 },
            new Product { Id = 3, JanCode = "999999", Name = "ポテトチップス", Price = 180m, TaxRate = 10 },
            new Product { Id = 4, JanCode = "111111", Name = "ティッシュ", Price = 200m, TaxRate = 10 },
            new Product { Id = 5, JanCode = "222222", Name = "コーヒー 熱 350ml", Price = 110m, TaxRate = 10 }
        );
        dbContext.SaveChanges();
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task LookupByBarcodeAsync_ExistingCode_ReturnsProduct()
    {
        var result = await _sut.LookupByBarcodeAsync("777777");
        Assert.NotNull(result);
        Assert.Equal("おにぎり 梅", result!.Name);
        Assert.Equal(120m, result.Price);
        Assert.Equal(8, result.TaxRate);
    }

    [Fact]
    public async Task LookupByBarcodeAsync_NonExistingCode_ReturnsNull()
    {
        var result = await _sut.LookupByBarcodeAsync("000000");
        Assert.Null(result);
    }

    [Fact]
    public async Task LookupByBarcodeAsync_EmptyString_ReturnsNull()
    {
        var result = await _sut.LookupByBarcodeAsync("");
        Assert.Null(result);
    }

    [Fact]
    public async Task LookupByBarcodeAsync_AllProducts_Exist()
    {
        var codes = new[] { "777777", "888888", "999999", "111111", "222222" };
        var expectedNames = new[] { "おにぎり 梅", "緑茶 500ml", "ポテトチップス", "ティッシュ", "コーヒー 熱 350ml" };

        foreach (var (code, expectedName) in codes.Zip(expectedNames))
        {
            var result = await _sut.LookupByBarcodeAsync(code);
            Assert.NotNull(result);
            Assert.Equal(expectedName, result!.Name);
        }
    }

    [Fact]
    public async Task LookupByBarcodeAsync_ReturnsCorrectTaxRate()
    {
        var onigiri = await _sut.LookupByBarcodeAsync("777777");
        Assert.Equal(8, onigiri!.TaxRate);

        var chips = await _sut.LookupByBarcodeAsync("999999");
        Assert.Equal(10, chips!.TaxRate);
    }

    [Fact]
    public async Task LookupByBarcodeAsync_PartialMatch_ReturnsNull()
    {
        var result = await _sut.LookupByBarcodeAsync("77777");
        Assert.Null(result);
    }

    [Fact]
    public async Task LookupByBarcodeAsync_DuplicateBarcode_ReturnsFirstMatch()
    {
        using var dbContext = new PosDbContext(_options);
        dbContext.Products.Add(
            new Product { Id = 6, JanCode = "777777", Name = "おにぎり 塩", Price = 130m, TaxRate = 8 }
        );
        dbContext.SaveChanges();

        var result = await _sut.LookupByBarcodeAsync("777777");
        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
    }

    [Fact]
    public async Task LookupByBarcodeAsync_ProductWithZeroPrice_ReturnsProduct()
    {
        using var dbContext = new PosDbContext(_options);
        dbContext.Products.Add(
            new Product { Id = 99, JanCode = "000000", Name = "テスト商品", Price = 0m, TaxRate = 10 }
        );
        dbContext.SaveChanges();

        var result = await _sut.LookupByBarcodeAsync("000000");
        Assert.NotNull(result);
        Assert.Equal(0m, result!.Price);
    }

    [Fact]
    public async Task LookupByBarcodeAsync_VeryLongBarcode_ReturnsNull()
    {
        var longBarcode = new string('1', 100);
        var result = await _sut.LookupByBarcodeAsync(longBarcode);
        Assert.Null(result);
    }

    [Fact]
    public async Task LookupByBarcodeAsync_ProductsTableHas5Rows()
    {
        using var dbContext = new PosDbContext(_options);
        var count = await dbContext.Products.CountAsync();
        Assert.Equal(5, count);
    }
}
