using ConveniencePos.Data.Seed;
using ConveniencePos.Services;
using ConveniencePos.ViewModels;
using Moq;
using Xunit;

namespace ConveniencePos.Tests.ViewModels;

public class SeedDataTests
{
    [Fact]
    public void SeedData_HasExactly5Products()
    {
        var products = ProductSeedData.GetProducts();
        Assert.Equal(5, products.Length);
    }

    [Fact]
    public void SeedData_Onigiri_Price120_Tax8()
    {
        var products = ProductSeedData.GetProducts();
        var product = Array.Find(products, p => p.JanCode == "777777");
        Assert.NotNull(product);
        Assert.Equal("おにぎり 梅", product.Name);
        Assert.Equal(120m, product.Price);
        Assert.Equal(8, product.TaxRate);
    }

    [Fact]
    public void SeedData_GreenTea_Price150_Tax8()
    {
        var products = ProductSeedData.GetProducts();
        var product = Array.Find(products, p => p.JanCode == "888888");
        Assert.NotNull(product);
        Assert.Equal("緑茶 500ml", product.Name);
        Assert.Equal(150m, product.Price);
        Assert.Equal(8, product.TaxRate);
    }

    [Fact]
    public void SeedData_PotatoChips_Price180_Tax10()
    {
        var products = ProductSeedData.GetProducts();
        var product = Array.Find(products, p => p.JanCode == "999999");
        Assert.NotNull(product);
        Assert.Equal("ポテトチップス", product.Name);
        Assert.Equal(180m, product.Price);
        Assert.Equal(10, product.TaxRate);
    }

    [Fact]
    public void SeedData_Tissue_Price200_Tax10()
    {
        var products = ProductSeedData.GetProducts();
        var product = Array.Find(products, p => p.JanCode == "111111");
        Assert.NotNull(product);
        Assert.Equal("ティッシュ", product.Name);
        Assert.Equal(200m, product.Price);
        Assert.Equal(10, product.TaxRate);
    }

    [Fact]
    public void SeedData_Coffee_Price110_Tax10()
    {
        var products = ProductSeedData.GetProducts();
        var product = Array.Find(products, p => p.JanCode == "222222");
        Assert.NotNull(product);
        Assert.Equal("コーヒー 熱 350ml", product.Name);
        Assert.Equal(110m, product.Price);
        Assert.Equal(10, product.TaxRate);
    }

    [Fact]
    public void SeedData_AllProductsHaveUniqueJanCodes()
    {
        var products = ProductSeedData.GetProducts();
        var janCodes = products.Select(p => p.JanCode).ToList();
        Assert.Equal(janCodes.Count, janCodes.Distinct().Count());
    }

    [Fact]
    public void SeedData_AllProductsHavePositivePrices()
    {
        var products = ProductSeedData.GetProducts();
        foreach (var product in products)
        {
            Assert.True(product.Price > 0, $"Product {product.Name} must have positive price");
        }
    }

    [Fact]
    public void SeedData_AllProductsHaveValidTaxRates()
    {
        var products = ProductSeedData.GetProducts();
        foreach (var product in products)
        {
            Assert.Contains(product.TaxRate, new[] { 8, 10 });
        }
    }
}

public class PerProductTaxCalculationTests
{
    private static MainViewModel CreateViewModel(
        Mock<IBarcodeService>? barcodeMock = null,
        Mock<ITransactionService>? transactionMock = null,
        Mock<IReceiptService>? receiptMock = null)
    {
        barcodeMock ??= new Mock<IBarcodeService>();
        transactionMock ??= new Mock<ITransactionService>();
        receiptMock ??= new Mock<IReceiptService>();
        var loggerMock = new Mock<Microsoft.Extensions.Logging.ILogger<MainViewModel>>();
        return new MainViewModel(
            barcodeMock.Object,
            transactionMock.Object,
            receiptMock.Object,
            loggerMock.Object);
    }

    [Fact]
    public void Tissue_SingleItem_UnitPrice_Is200()
    {
        var item = new CartItemViewModel(4, "ティッシュ", 200m, 10, 1);
        Assert.Equal(200m, item.UnitPrice);
    }

    [Fact]
    public void Tissue_SingleItem_LineTotal_Is200()
    {
        var item = new CartItemViewModel(4, "ティッシュ", 200m, 10, 1);
        Assert.Equal(200m, item.LineTotal);
    }

    [Fact]
    public void Tissue_SingleItem_LineTotalWithTax_Is220()
    {
        var item = new CartItemViewModel(4, "ティッシュ", 200m, 10, 1);
        Assert.Equal(220m, item.LineTotalWithTax);
    }

    [Fact]
    public void Tissue_TwoItems_LineTotalWithTax_Is440()
    {
        var item = new CartItemViewModel(4, "ティッシュ", 200m, 10, 2);
        Assert.Equal(440m, item.LineTotalWithTax);
    }

    [Fact]
    public void Onigiri_SingleItem_LineTotalWithTax_Is129()
    {
        var item = new CartItemViewModel(1, "おにぎり 梅", 120m, 8, 1);
        Assert.Equal(129m, item.LineTotalWithTax);
    }

    [Fact]
    public void GreenTea_SingleItem_LineTotalWithTax_Is162()
    {
        var item = new CartItemViewModel(2, "緑茶 500ml", 150m, 8, 1);
        Assert.Equal(162m, item.LineTotalWithTax);
    }

    [Fact]
    public void PotatoChips_SingleItem_LineTotalWithTax_Is198()
    {
        var item = new CartItemViewModel(3, "ポテトチップス", 180m, 10, 1);
        Assert.Equal(198m, item.LineTotalWithTax);
    }

    [Fact]
    public void Coffee_SingleItem_LineTotalWithTax_Is121()
    {
        var item = new CartItemViewModel(5, "コーヒー 熱 350ml", 110m, 10, 1);
        Assert.Equal(121m, item.LineTotalWithTax);
    }

    [Fact]
    public void AllSeedProducts_CartWithOneEach_TotalsAreCorrect()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり 梅", 120m, 8, 1));
        vm.CartItems.Add(new CartItemViewModel(2, "緑茶 500ml", 150m, 8, 1));
        vm.CartItems.Add(new CartItemViewModel(3, "ポテトチップス", 180m, 10, 1));
        vm.CartItems.Add(new CartItemViewModel(4, "ティッシュ", 200m, 10, 1));
        vm.CartItems.Add(new CartItemViewModel(5, "コーヒー 熱 350ml", 110m, 10, 1));
        vm.RefreshTotals();

        Assert.Equal(760m, vm.Subtotal);
        Assert.Equal(270m, vm.TaxableAmount8);
        Assert.Equal(490m, vm.TaxableAmount10);
        Assert.Equal(21m, vm.TaxAmount8);
        Assert.Equal(49m, vm.TaxAmount10);
        Assert.Equal(70m, vm.TaxAmount);
        Assert.Equal(830m, vm.TotalAmount);
    }

    [Fact]
    public void Tissue_CartWithOne_TotalsAreCorrect()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(4, "ティッシュ", 200m, 10, 1));
        vm.RefreshTotals();

        Assert.Equal(200m, vm.Subtotal);
        Assert.Equal(0m, vm.TaxableAmount8);
        Assert.Equal(200m, vm.TaxableAmount10);
        Assert.Equal(0m, vm.TaxAmount8);
        Assert.Equal(20m, vm.TaxAmount10);
        Assert.Equal(20m, vm.TaxAmount);
        Assert.Equal(220m, vm.TotalAmount);
    }

    [Fact]
    public void Onigiri_CartWithOne_TotalsAreCorrect()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり 梅", 120m, 8, 1));
        vm.RefreshTotals();

        Assert.Equal(120m, vm.Subtotal);
        Assert.Equal(120m, vm.TaxableAmount8);
        Assert.Equal(0m, vm.TaxableAmount10);
        Assert.Equal(9m, vm.TaxAmount8);
        Assert.Equal(0m, vm.TaxAmount10);
        Assert.Equal(9m, vm.TaxAmount);
        Assert.Equal(129m, vm.TotalAmount);
    }
}

public class DisplayFormatTests
{
    [Fact]
    public void UnitPrice_FormatsWithYenSign()
    {
        var item = new CartItemViewModel(1, "テスト", 120m, 8);
        var formatted = $"¥{item.UnitPrice:N0}";
        Assert.Equal("¥120", formatted);
    }

    [Fact]
    public void LineTotalWithTax_FormatsWithYenSign()
    {
        var item = new CartItemViewModel(1, "テスト", 120m, 8, 2);
        var formatted = $"¥{item.LineTotalWithTax:N0}";
        Assert.Equal("¥259", formatted);
    }

    [Fact]
    public void Tissue_FormatsAs200Not220()
    {
        var item = new CartItemViewModel(4, "ティッシュ", 200m, 10, 1);
        var unitPriceFormatted = $"¥{item.UnitPrice:N0}";
        var lineTotalFormatted = $"¥{item.LineTotalWithTax:N0}";
        Assert.Equal("¥200", unitPriceFormatted);
        Assert.Equal("¥220", lineTotalFormatted);
    }

    [Fact]
    public void ReceiptService_GenerateReceipt_ContainsYenSign()
    {
        var receiptService = new ReceiptService(
            storeName: "テスト店舗",
            registerNumber: "レジ#01",
            operatorName: "テスト担当",
            outputDirectory: "Desktop");

        var context = new ReceiptContext(
            1,
            DateTime.UtcNow,
            [new ReceiptItem("テスト商品", 1, 100m, 10)],
            100m, 0m, 100m, 0m, 10m, 10m, 110m, 200m, 90m);

        var receipt = receiptService.GenerateReceipt(context);
        Assert.Contains("¥", receipt);
        Assert.Contains("¥100", receipt);
        Assert.Contains("¥10", receipt);
        Assert.Contains("¥110", receipt);
        Assert.Contains("¥200", receipt);
        Assert.Contains("¥90", receipt);
    }
}
