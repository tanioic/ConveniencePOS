using ConveniencePos.Models;
using Xunit;

namespace ConveniencePos.Tests.Models;

public class ProductTests
{
    [Fact]
    public void Product_DefaultValues_AreInitializedCorrectly()
    {
        var product = new Product();

        Assert.Equal(0, product.Id);
        Assert.Equal(string.Empty, product.JanCode);
        Assert.Equal(string.Empty, product.Name);
        Assert.Equal(0m, product.Price);
        Assert.Equal(0, product.TaxRate);
    }

    [Fact]
    public void Product_SetProperties_ReturnsCorrectValues()
    {
        var product = new Product
        {
            Id = 1,
            JanCode = "777777",
            Name = "おにぎり 梅",
            Price = 120m,
            TaxRate = 8
        };

        Assert.Equal(1, product.Id);
        Assert.Equal("777777", product.JanCode);
        Assert.Equal("おにぎり 梅", product.Name);
        Assert.Equal(120m, product.Price);
        Assert.Equal(8, product.TaxRate);
    }

    [Fact]
    public void Product_TaxRate10_IsValid()
    {
        var product = new Product { TaxRate = 10 };

        Assert.Equal(10, product.TaxRate);
    }

    [Fact]
    public void Product_Price_CanBeDecimal()
    {
        var product = new Product { Price = 199.99m };

        Assert.Equal(199.99m, product.Price);
    }

    [Fact]
    public void Product_JanCode_CanBeEmpty()
    {
        var product = new Product { JanCode = "" };

        Assert.Equal(string.Empty, product.JanCode);
    }
}

public class TransactionTests
{
    [Fact]
    public void Transaction_DefaultValues_AreInitializedCorrectly()
    {
        var transaction = new Transaction();

        Assert.Equal(0, transaction.Id);
        Assert.Equal(default(DateTime), transaction.CreatedAt);
        Assert.Equal(0m, transaction.TotalAmount);
        Assert.Equal(0m, transaction.TaxAmount);
        Assert.NotNull(transaction.Items);
        Assert.Empty(transaction.Items);
    }

    [Fact]
    public void Transaction_Items_IsEmptyListByDefault()
    {
        var transaction = new Transaction();

        Assert.IsType<List<TransactionItem>>(transaction.Items);
        Assert.Empty(transaction.Items);
    }

    [Fact]
    public void Transaction_SetProperties_ReturnsCorrectValues()
    {
        var now = DateTime.Now;
        var transaction = new Transaction
        {
            Id = 100,
            CreatedAt = now,
            TotalAmount = 669m,
            TaxAmount = 59m
        };

        Assert.Equal(100, transaction.Id);
        Assert.Equal(now, transaction.CreatedAt);
        Assert.Equal(669m, transaction.TotalAmount);
        Assert.Equal(59m, transaction.TaxAmount);
    }

    [Fact]
    public void Transaction_CanAddItems()
    {
        var transaction = new Transaction();
        var item = new TransactionItem
        {
            TransactionId = 1,
            ProductId = 1,
            Quantity = 2,
            UnitPrice = 120m,
            AppliedTaxRate = 8
        };

        transaction.Items.Add(item);

        Assert.Single(transaction.Items);
        Assert.Equal(120m, transaction.Items.First().UnitPrice);
    }
}

public class TransactionItemTests
{
    [Fact]
    public void TransactionItem_DefaultValues_AreCorrect()
    {
        var item = new TransactionItem();

        Assert.Equal(0, item.Id);
        Assert.Equal(0, item.TransactionId);
        Assert.Equal(0, item.ProductId);
        Assert.Equal(0, item.Quantity);
        Assert.Equal(0m, item.UnitPrice);
        Assert.Equal(0, item.AppliedTaxRate);
    }

    [Fact]
    public void TransactionItem_SetProperties_ReturnsCorrectValues()
    {
        var item = new TransactionItem
        {
            Id = 10,
            TransactionId = 100,
            ProductId = 5,
            Quantity = 3,
            UnitPrice = 150m,
            AppliedTaxRate = 10
        };

        Assert.Equal(10, item.Id);
        Assert.Equal(100, item.TransactionId);
        Assert.Equal(5, item.ProductId);
        Assert.Equal(3, item.Quantity);
        Assert.Equal(150m, item.UnitPrice);
        Assert.Equal(10, item.AppliedTaxRate);
    }

    [Fact]
    public void TransactionItem_Quantity_CanBeZero()
    {
        var item = new TransactionItem { Quantity = 0 };

        Assert.Equal(0, item.Quantity);
    }

    [Fact]
    public void TransactionItem_TaxRate8_IsReducedRate()
    {
        var item = new TransactionItem { AppliedTaxRate = 8 };

        Assert.Equal(8, item.AppliedTaxRate);
    }

    [Fact]
    public void TransactionItem_TaxRate10_IsStandardRate()
    {
        var item = new TransactionItem { AppliedTaxRate = 10 };

        Assert.Equal(10, item.AppliedTaxRate);
    }
}
