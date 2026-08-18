using ConveniencePos.Models;
using Xunit;

namespace ConveniencePos.Tests.Models;

public class ProductTests
{
    [Fact]
    public void DefaultJanCode_IsEmpty()
    {
        var product = new Product();
        Assert.Equal(string.Empty, product.JanCode);
    }

    [Fact]
    public void DefaultName_IsEmpty()
    {
        var product = new Product();
        Assert.Equal(string.Empty, product.Name);
    }

    [Fact]
    public void SetProperty_Id()
    {
        var product = new Product { Id = 5 };
        Assert.Equal(5, product.Id);
    }

    [Fact]
    public void SetProperty_JanCode()
    {
        var product = new Product { JanCode = "777777" };
        Assert.Equal("777777", product.JanCode);
    }

    [Fact]
    public void SetProperty_Name()
    {
        var product = new Product { Name = "おにぎり 梅" };
        Assert.Equal("おにぎり 梅", product.Name);
    }

    [Fact]
    public void SetProperty_Price()
    {
        var product = new Product { Price = 120m };
        Assert.Equal(120m, product.Price);
    }

    [Fact]
    public void SetProperty_TaxRate()
    {
        var product = new Product { TaxRate = 8 };
        Assert.Equal(8, product.TaxRate);
    }

    [Fact]
    public void TaxRate_CanBe10()
    {
        var product = new Product { TaxRate = 10 };
        Assert.Equal(10, product.TaxRate);
    }

    [Fact]
    public void Price_CanBeZero()
    {
        var product = new Product { Price = 0m };
        Assert.Equal(0m, product.Price);
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        var product = new Product
        {
            Id = 1,
            JanCode = "999999",
            Name = "テスト商品",
            Price = 250m,
            TaxRate = 10
        };

        Assert.Equal(1, product.Id);
        Assert.Equal("999999", product.JanCode);
        Assert.Equal("テスト商品", product.Name);
        Assert.Equal(250m, product.Price);
        Assert.Equal(10, product.TaxRate);
    }
}

public class TransactionTests
{
    [Fact]
    public void DefaultItems_IsEmptyList()
    {
        var transaction = new Transaction();
        Assert.NotNull(transaction.Items);
        Assert.Empty(transaction.Items);
    }

    [Fact]
    public void SetProperty_CreatedAt()
    {
        var now = DateTime.UtcNow;
        var transaction = new Transaction { CreatedAt = now };
        Assert.Equal(now, transaction.CreatedAt);
    }

    [Fact]
    public void SetProperty_TotalAmount()
    {
        var transaction = new Transaction { TotalAmount = 327m };
        Assert.Equal(327m, transaction.TotalAmount);
    }

    [Fact]
    public void SetProperty_TaxAmount()
    {
        var transaction = new Transaction { TaxAmount = 27m };
        Assert.Equal(27m, transaction.TaxAmount);
    }
}

public class TransactionItemTests
{
    [Fact]
    public void SetProperty_TransactionId()
    {
        var item = new TransactionItem { TransactionId = 1 };
        Assert.Equal(1, item.TransactionId);
    }

    [Fact]
    public void SetProperty_ProductId()
    {
        var item = new TransactionItem { ProductId = 3 };
        Assert.Equal(3, item.ProductId);
    }

    [Fact]
    public void SetProperty_Quantity()
    {
        var item = new TransactionItem { Quantity = 5 };
        Assert.Equal(5, item.Quantity);
    }

    [Fact]
    public void SetProperty_UnitPrice()
    {
        var item = new TransactionItem { UnitPrice = 180m };
        Assert.Equal(180m, item.UnitPrice);
    }

    [Fact]
    public void SetProperty_AppliedTaxRate()
    {
        var item = new TransactionItem { AppliedTaxRate = 10 };
        Assert.Equal(10, item.AppliedTaxRate);
    }

    [Fact]
    public void AllProperties_CanBeSetTogether()
    {
        var item = new TransactionItem
        {
            TransactionId = 1,
            ProductId = 3,
            Quantity = 2,
            UnitPrice = 180m,
            AppliedTaxRate = 10
        };

        Assert.Equal(1, item.TransactionId);
        Assert.Equal(3, item.ProductId);
        Assert.Equal(2, item.Quantity);
        Assert.Equal(180m, item.UnitPrice);
        Assert.Equal(10, item.AppliedTaxRate);
    }
}
