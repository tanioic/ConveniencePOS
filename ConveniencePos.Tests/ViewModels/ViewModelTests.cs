using ConveniencePos.Services;
using ConveniencePos.ViewModels;
using Moq;
using Xunit;

namespace ConveniencePos.Tests.ViewModels;

public class CartItemViewModelTests
{
    [Fact]
    public void LineTotal_CalculatesCorrectly()
    {
        var item = new CartItemViewModel(1, "テスト", 100m, 8, 3);
        Assert.Equal(300m, item.LineTotal);
    }

    [Fact]
    public void LineTotalWithTax_8Percent()
    {
        var item = new CartItemViewModel(1, "おにぎり", 120m, 8, 2);
        Assert.Equal(259m, item.LineTotalWithTax);
    }

    [Fact]
    public void LineTotalWithTax_10Percent()
    {
        var item = new CartItemViewModel(1, "チップス", 180m, 10, 1);
        Assert.Equal(198m, item.LineTotalWithTax);
    }

    [Fact]
    public void LineTotal_UpdatesOnQuantityChange()
    {
        var item = new CartItemViewModel(1, "テスト", 100m, 8, 1);
        Assert.Equal(100m, item.LineTotal);
        item.Quantity = 3;
        Assert.Equal(300m, item.LineTotal);
    }

    [Fact]
    public void LineTotalWithTax_UpdatesOnQuantityChange()
    {
        var item = new CartItemViewModel(1, "おにぎり", 120m, 8, 1);
        Assert.Equal(129m, item.LineTotalWithTax);
        item.Quantity = 2;
        Assert.Equal(259m, item.LineTotalWithTax);
    }

    [Fact]
    public void Quantity_BelowOne_ThrowsException()
    {
        var item = new CartItemViewModel(1, "テスト", 100m, 8, 1);
        Assert.Throws<ArgumentOutOfRangeException>(() => item.Quantity = 0);
    }

    [Fact]
    public void LineTotal_ZeroQuantity_ReturnsZero()
    {
        var item = new CartItemViewModel(1, "テスト", 100m, 8, 1);
        item.Quantity = 1;
        Assert.Equal(100m, item.LineTotal);
    }

    [Fact]
    public void LineTotalWithTax_FloorRounding()
    {
        var item = new CartItemViewModel(1, "テスト", 100m, 8, 3);
        Assert.Equal(324m, item.LineTotalWithTax);
    }

    [Fact]
    public void DefaultQuantity_IsOne()
    {
        var item = new CartItemViewModel(1, "テスト", 100m, 8);
        Assert.Equal(1, item.Quantity);
    }
}

public class MainViewModelTests
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
    public void EmptyCart_Subtotal_IsZero()
    {
        var vm = CreateViewModel();
        Assert.Equal(0m, vm.Subtotal);
    }

    [Fact]
    public void EmptyCart_TotalAmount_IsZero()
    {
        var vm = CreateViewModel();
        Assert.Equal(0m, vm.TotalAmount);
    }

    [Fact]
    public void EmptyCart_Change_IsZero()
    {
        var vm = CreateViewModel();
        Assert.Equal(0m, vm.Change);
    }

    [Fact]
    public void SingleItem8Percent_Subtotal()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.RefreshTotals();
        Assert.Equal(120m, vm.Subtotal);
    }

    [Fact]
    public void SingleItem8Percent_TaxableAmount8()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.RefreshTotals();
        Assert.Equal(120m, vm.TaxableAmount8);
    }

    [Fact]
    public void SingleItem8Percent_TaxableAmount10_IsZero()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.RefreshTotals();
        Assert.Equal(0m, vm.TaxableAmount10);
    }

    [Fact]
    public void SingleItem8Percent_TaxAmount8()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.RefreshTotals();
        Assert.Equal(9m, vm.TaxAmount8);
    }

    [Fact]
    public void SingleItem8Percent_TotalAmount()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.RefreshTotals();
        Assert.Equal(129m, vm.TotalAmount);
    }

    [Fact]
    public void SingleItem10Percent_TaxAmount10()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "チップス", 180m, 10));
        vm.RefreshTotals();
        Assert.Equal(18m, vm.TaxAmount10);
    }

    [Fact]
    public void SingleItem10Percent_TotalAmount()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "チップス", 180m, 10));
        vm.RefreshTotals();
        Assert.Equal(198m, vm.TotalAmount);
    }

    [Fact]
    public void TwoItems8Percent_Subtotal()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8, 2));
        vm.RefreshTotals();
        Assert.Equal(240m, vm.Subtotal);
    }

    [Fact]
    public void TwoItems8Percent_TaxAmount8()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8, 2));
        vm.RefreshTotals();
        Assert.Equal(19m, vm.TaxAmount8);
    }

    [Fact]
    public void Change_CalculatesCorrectly()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.RefreshTotals();
        vm.ReceivedAmount = 200m;
        Assert.Equal(71m, vm.Change);
    }

    [Fact]
    public void Change_InsufficientAmount_ReturnsZero()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.RefreshTotals();
        vm.ReceivedAmount = 100m;
        Assert.Equal(0m, vm.Change);
    }

    [Fact]
    public void MixedTax_Subtotal()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.CartItems.Add(new CartItemViewModel(2, "チップス", 180m, 10));
        vm.RefreshTotals();
        Assert.Equal(300m, vm.Subtotal);
    }

    [Fact]
    public void MixedTax_TaxAmount8()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.CartItems.Add(new CartItemViewModel(2, "チップス", 180m, 10));
        vm.RefreshTotals();
        Assert.Equal(9m, vm.TaxAmount8);
    }

    [Fact]
    public void MixedTax_TaxAmount10()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.CartItems.Add(new CartItemViewModel(2, "チップス", 180m, 10));
        vm.RefreshTotals();
        Assert.Equal(18m, vm.TaxAmount10);
    }

    [Fact]
    public void MixedTax_TotalAmount()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.CartItems.Add(new CartItemViewModel(2, "チップス", 180m, 10));
        vm.RefreshTotals();
        Assert.Equal(327m, vm.TotalAmount);
    }

    [Fact]
    public void MixedTax_TaxAmount()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.CartItems.Add(new CartItemViewModel(2, "チップス", 180m, 10));
        vm.RefreshTotals();
        Assert.Equal(27m, vm.TaxAmount);
    }

    [Fact]
    public void QuantityChange_UpdatesTotals()
    {
        var vm = CreateViewModel();
        var item = new CartItemViewModel(1, "おにぎり", 120m, 8, 1);
        vm.CartItems.Add(item);
        vm.RefreshTotals();
        Assert.Equal(129m, vm.TotalAmount);

        item.Quantity = 2;
        vm.RefreshTotals();
        Assert.Equal(259m, vm.TotalAmount);
    }

    [Fact]
    public void AddItemCommand_AddsNewProduct()
    {
        var barcodeMock = new Mock<IBarcodeService>();
        barcodeMock.Setup(b => b.LookupByBarcodeAsync("777777", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConveniencePos.Models.Product { Id = 1, JanCode = "777777", Name = "おにぎり 梅", Price = 120m, TaxRate = 8 });

        var vm = CreateViewModel(barcodeMock: barcodeMock);
        vm.BarcodeInput = "777777";
        vm.AddItemCommand.Execute(null);

        Assert.Single(vm.CartItems);
        Assert.Equal("おにぎり 梅", vm.CartItems[0].Name);
    }

    [Fact]
    public async Task AddItemCommand_DuplicateProduct_IncrementsQuantity()
    {
        var barcodeMock = new Mock<IBarcodeService>();
        barcodeMock.Setup(b => b.LookupByBarcodeAsync("777777", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConveniencePos.Models.Product { Id = 1, JanCode = "777777", Name = "おにぎり 梅", Price = 120m, TaxRate = 8 });

        var vm = CreateViewModel(barcodeMock: barcodeMock);
        vm.BarcodeInput = "777777";
        await vm.AddItemCommand.ExecuteAsync(null);
        vm.BarcodeInput = "777777";
        await vm.AddItemCommand.ExecuteAsync(null);

        Assert.Single(vm.CartItems);
        Assert.Equal(2, vm.CartItems[0].Quantity);
    }

    [Fact]
    public void AddItemCommand_ProductNotFound_KeepsCartUnchanged()
    {
        var barcodeMock = new Mock<IBarcodeService>();
        barcodeMock.Setup(b => b.LookupByBarcodeAsync("000000", It.IsAny<CancellationToken>()))
            .ReturnsAsync((ConveniencePos.Models.Product?)null);

        var vm = CreateViewModel(barcodeMock: barcodeMock);
        vm.BarcodeInput = "000000";
        vm.AddItemCommand.Execute(null);

        Assert.Empty(vm.CartItems);
    }

    [Fact]
    public async Task ConfirmTransactionCommand_SavesTransaction()
    {
        var transactionMock = new Mock<ITransactionService>();
        transactionMock.Setup(t => t.SaveTransactionAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<IReadOnlyList<ConveniencePos.Models.TransactionItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConveniencePos.Models.Transaction { Id = 1, CreatedAt = DateTime.UtcNow, TotalAmount = 259m, TaxAmount = 19m });

        var vm = CreateViewModel(transactionMock: transactionMock);
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8, 2));
        vm.RefreshTotals();
        vm.ReceivedAmount = 300m;

        await vm.ConfirmTransactionCommand.ExecuteAsync(null);

        transactionMock.Verify(t => t.SaveTransactionAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<IReadOnlyList<ConveniencePos.Models.TransactionItem>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ConfirmTransactionCommand_ClearsCart()
    {
        var transactionMock = new Mock<ITransactionService>();
        transactionMock.Setup(t => t.SaveTransactionAsync(It.IsAny<decimal>(), It.IsAny<decimal>(), It.IsAny<IReadOnlyList<ConveniencePos.Models.TransactionItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ConveniencePos.Models.Transaction { Id = 1, CreatedAt = DateTime.UtcNow, TotalAmount = 259m, TaxAmount = 19m });

        var vm = CreateViewModel(transactionMock: transactionMock);
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8, 2));
        vm.RefreshTotals();
        vm.ReceivedAmount = 300m;

        await vm.ConfirmTransactionCommand.ExecuteAsync(null);

        Assert.Empty(vm.CartItems);
        Assert.Equal(0m, vm.ReceivedAmount);
    }

    [Fact]
    public void ConfirmTransactionCommand_DisabledWhenCartEmpty()
    {
        var vm = CreateViewModel();
        Assert.False(vm.ConfirmTransactionCommand.CanExecute(null));
    }

    [Fact]
    public void ConfirmTransactionCommand_DisabledWhenInsufficientAmount()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.RefreshTotals();
        vm.ReceivedAmount = 50m;
        Assert.False(vm.ConfirmTransactionCommand.CanExecute(null));
    }

    [Fact]
    public void ConfirmTransactionCommand_EnabledWhenSufficientAmount()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8));
        vm.RefreshTotals();
        vm.ReceivedAmount = 200m;
        Assert.True(vm.ConfirmTransactionCommand.CanExecute(null));
    }

    [Fact]
    public void TaxAmount_IsFloor_NotRound()
    {
        var vm = CreateViewModel();
        vm.CartItems.Add(new CartItemViewModel(1, "おにぎり", 120m, 8, 1));
        vm.RefreshTotals();
        Assert.Equal(9m, vm.TaxAmount8);
        Assert.Equal(9m, vm.TaxAmount);
    }
}
