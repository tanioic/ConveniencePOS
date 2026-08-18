using ConveniencePos.Data;
using ConveniencePos.Models;
using ConveniencePos.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace ConveniencePos.Tests.Services;

public class TransactionServiceTests : IDisposable
{
    private readonly IDbContextFactory<PosDbContext> _contextFactory;
    private readonly TransactionService _transactionService;

    public TransactionServiceTests()
    {
        var options = new DbContextOptionsBuilder<PosDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var factoryMock = new Mock<IDbContextFactory<PosDbContext>>();
        factoryMock.Setup(f => f.CreateDbContextAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new PosDbContext(options));

        _contextFactory = factoryMock.Object;

        var loggerMock = new Mock<ILogger<TransactionService>>();
        _transactionService = new TransactionService(_contextFactory, loggerMock.Object);
    }

    [Fact]
    public async Task SaveTransaction_NullItems_ThrowsException()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(
            async () => await _transactionService.SaveTransactionAsync(100m, 10m, null!));
    }

    [Fact]
    public async Task SaveTransaction_EmptyItems_ThrowsException()
    {
        await Assert.ThrowsAsync<ArgumentException>(
            async () => await _transactionService.SaveTransactionAsync(100m, 10m, new List<TransactionItem>()));
    }

    [Fact]
    public async Task SaveTransaction_NegativeTotalAmount_ThrowsException()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 1, UnitPrice = 100m, AppliedTaxRate = 8 }
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _transactionService.SaveTransactionAsync(-1m, 10m, items));
    }

    [Fact]
    public async Task SaveTransaction_NegativeTaxAmount_ThrowsException()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 1, UnitPrice = 100m, AppliedTaxRate = 8 }
        };

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(
            async () => await _transactionService.SaveTransactionAsync(100m, -1m, items));
    }

    [Fact]
    public async Task SaveTransaction_ValidTransaction_ReturnsTransaction()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 2, UnitPrice = 120m, AppliedTaxRate = 8 }
        };

        var result = await _transactionService.SaveTransactionAsync(259m, 19m, items);

        Assert.NotNull(result);
        Assert.Equal(259m, result.TotalAmount);
        Assert.Equal(19m, result.TaxAmount);
        Assert.True(result.Id > 0);
    }

    [Fact]
    public async Task SaveTransaction_ItemsArePersisted()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 1, UnitPrice = 120m, AppliedTaxRate = 8 },
            new() { ProductId = 3, Quantity = 1, UnitPrice = 180m, AppliedTaxRate = 10 }
        };

        var saved = await _transactionService.SaveTransactionAsync(327m, 27m, items);

        await using var db = await _contextFactory.CreateDbContextAsync();
        var loaded = await db.Transactions
            .Include(t => t.Items)
            .FirstOrDefaultAsync(t => t.Id == saved.Id);

        Assert.NotNull(loaded);
        Assert.Equal(2, loaded.Items.Count);
    }

    [Fact]
    public async Task SaveTransaction_ZeroAmount_Succeeds()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 1, UnitPrice = 100m, AppliedTaxRate = 8 }
        };

        var result = await _transactionService.SaveTransactionAsync(0m, 0m, items);
        Assert.NotNull(result);
    }

    [Fact]
    public async Task SaveTransaction_VerifyCreatedAt()
    {
        var items = new List<TransactionItem>
        {
            new() { ProductId = 1, Quantity = 1, UnitPrice = 100m, AppliedTaxRate = 8 }
        };

        var before = DateTime.UtcNow;
        var result = await _transactionService.SaveTransactionAsync(100m, 8m, items);
        var after = DateTime.UtcNow;

        Assert.InRange(result.CreatedAt, before, after);
    }

    public void Dispose() { }
}
