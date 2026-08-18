using ConveniencePos.Data;
using ConveniencePos.Models;
using ConveniencePos.Services;
using ConveniencePos.Tests.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConveniencePos.Tests.Services;

public class TransactionServiceTests : IDisposable
{
    private readonly DbContextOptions<PosDbContext> _options;
    private readonly TestDbContextFactory _factory;
    private readonly TransactionService _sut;

    public TransactionServiceTests()
    {
        _options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _factory = new TestDbContextFactory(_options);
        var logger = new Mock<ILogger<TransactionService>>();
        _sut = new TransactionService(_factory, logger.Object);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    [Fact]
    public async Task SaveTransactionAsync_NullItems_ThrowsArgumentNullException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            () => _sut.SaveTransactionAsync(100m, 8m, null!));
    }

    [Fact]
    public async Task SaveTransactionAsync_EmptyItems_ThrowsArgumentException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            () => _sut.SaveTransactionAsync(100m, 8m, new List<TransactionItem>()));
    }

    [Fact]
    public async Task SaveTransactionAsync_NegativeTotalAmount_ThrowsArgumentOutOfRangeException()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 1, UnitPrice = 100m, AppliedTaxRate = 8 }
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.SaveTransactionAsync(-1m, 0m, items));
    }

    [Fact]
    public async Task SaveTransactionAsync_NegativeTaxAmount_ThrowsArgumentOutOfRangeException()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 1, UnitPrice = 100m, AppliedTaxRate = 8 }
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            () => _sut.SaveTransactionAsync(100m, -1m, items));
    }

    [Fact]
    public async Task SaveTransactionAsync_ValidItems_ReturnsTransaction()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 2, UnitPrice = 120m, AppliedTaxRate = 8 }
        };

        var result = await _sut.SaveTransactionAsync(258m, 18m, items);

        Assert.NotNull(result);
        Assert.Equal(258m, result.TotalAmount);
        Assert.Equal(18m, result.TaxAmount);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task SaveTransactionAsync_SavesToDatabase()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 1, UnitPrice = 120m, AppliedTaxRate = 8 }
        };

        await _sut.SaveTransactionAsync(129m, 9m, items);

        using var dbContext = new PosDbContext(_options);
        var transactions = await dbContext.Transactions.ToListAsync();
        Assert.Single(transactions);
        Assert.Equal(129m, transactions[0].TotalAmount);

        var txItems = await dbContext.TransactionItems.ToListAsync();
        Assert.Single(txItems);
        Assert.Equal(1, txItems[0].ProductId);
        Assert.Equal(1, txItems[0].Quantity);
        Assert.Equal(120m, txItems[0].UnitPrice);
        Assert.Equal(8, txItems[0].AppliedTaxRate);
    }

    [Fact]
    public async Task SaveTransactionAsync_MultipleItems_AllSaved()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 1, UnitPrice = 120m, AppliedTaxRate = 8 },
            new() { ProductId = 2, Quantity = 2, UnitPrice = 150m, AppliedTaxRate = 8 },
            new() { ProductId = 3, Quantity = 1, UnitPrice = 180m, AppliedTaxRate = 10 }
        };

        var result = await _sut.SaveTransactionAsync(600m, 57m, items);

        using var dbContext = new PosDbContext(_options);
        var txItems = await dbContext.TransactionItems.ToListAsync();
        Assert.Equal(3, txItems.Count);
    }

    [Fact]
    public async Task SaveTransactionAsync_ZeroAmount_Succeeds()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 0, UnitPrice = 100m, AppliedTaxRate = 8 }
        };

        var result = await _sut.SaveTransactionAsync(0m, 0m, items);

        Assert.NotNull(result);
        Assert.Equal(0m, result.TotalAmount);
    }
}
