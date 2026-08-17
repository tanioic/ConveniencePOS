using System.ComponentModel;
using ConveniencePos.ViewModels;
using Xunit;

namespace ConveniencePos.Tests.ViewModels;

public class CartItemViewModelTests
{
    [Fact]
    public void LineTotal_ReturnsUnitPriceTimesQuantity()
    {
        var item = new CartItemViewModel
        {
            ProductId = 1,
            Name = "おにぎり 梅",
            UnitPrice = 120m,
            TaxRate = 8,
            Quantity = 3
        };

        Assert.Equal(360m, item.LineTotal);
    }

    [Fact]
    public void LineTotal_ZeroQuantity_ReturnsZero()
    {
        var item = new CartItemViewModel
        {
            ProductId = 1,
            Name = "おにぎり 梅",
            UnitPrice = 120m,
            TaxRate = 8,
            Quantity = 0
        };

        Assert.Equal(0m, item.LineTotal);
    }

    [Fact]
    public void LineTotalWithTax_TaxRate8_CalculatesCorrectly()
    {
        var item = new CartItemViewModel
        {
            ProductId = 1,
            Name = "おにぎり 梅",
            UnitPrice = 120m,
            TaxRate = 8,
            Quantity = 1
        };

        // 120 * 1.08 = 129.6 -> Math.Floor = 129
        Assert.Equal(129m, item.LineTotalWithTax);
    }

    [Fact]
    public void LineTotalWithTax_TaxRate10_CalculatesCorrectly()
    {
        var item = new CartItemViewModel
        {
            ProductId = 3,
            Name = "ポテトチップス",
            UnitPrice = 180m,
            TaxRate = 10,
            Quantity = 1
        };

        // 180 * 1.10 = 198
        Assert.Equal(198m, item.LineTotalWithTax);
    }

    [Fact]
    public void LineTotalWithTax_MultipleQuantity_CalculatesCorrectly()
    {
        var item = new CartItemViewModel
        {
            ProductId = 2,
            Name = "お茶 500ml",
            UnitPrice = 150m,
            TaxRate = 8,
            Quantity = 2
        };

        // 150 * 2 = 300 (LineTotal)
        // 300 * 1.08 = 324 (LineTotalWithTax)
        Assert.Equal(300m, item.LineTotal);
        Assert.Equal(324m, item.LineTotalWithTax);
    }

    [Fact]
    public void QuantityChanged_RaisesPropertyChangedForLineTotal()
    {
        var item = new CartItemViewModel
        {
            ProductId = 1,
            Name = "テスト",
            UnitPrice = 100m,
            TaxRate = 8,
            Quantity = 1
        };

        var changedProperties = new List<string>();
        ((INotifyPropertyChanged)item).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != null)
                changedProperties.Add(e.PropertyName);
        };

        item.Quantity = 2;

        Assert.Contains(nameof(CartItemViewModel.LineTotal), changedProperties);
    }

    [Fact]
    public void QuantityChanged_RaisesPropertyChangedForLineTotalWithTax()
    {
        var item = new CartItemViewModel
        {
            ProductId = 1,
            Name = "テスト",
            UnitPrice = 100m,
            TaxRate = 10,
            Quantity = 1
        };

        var changedProperties = new List<string>();
        ((INotifyPropertyChanged)item).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != null)
                changedProperties.Add(e.PropertyName);
        };

        item.Quantity = 3;

        Assert.Contains(nameof(CartItemViewModel.LineTotalWithTax), changedProperties);
    }

    [Fact]
    public void QuantityChanged_UpdatesLineTotal()
    {
        var item = new CartItemViewModel
        {
            ProductId = 1,
            Name = "テスト",
            UnitPrice = 100m,
            TaxRate = 8,
            Quantity = 1
        };

        item.Quantity = 5;

        Assert.Equal(500m, item.LineTotal);
    }

    [Fact]
    public void QuantityChanged_UpdatesLineTotalWithTax()
    {
        var item = new CartItemViewModel
        {
            ProductId = 1,
            Name = "テスト",
            UnitPrice = 100m,
            TaxRate = 10,
            Quantity = 1
        };

        item.Quantity = 3;

        // 100 * 3 = 300 -> 300 * 1.10 = 330
        Assert.Equal(330m, item.LineTotalWithTax);
    }

    [Fact]
    public void LineTotalWithTax_FloorTruncation_CalculatesCorrectly()
    {
        var item = new CartItemViewModel
        {
            ProductId = 1,
            Name = "テスト",
            UnitPrice = 111m,
            TaxRate = 8,
            Quantity = 1
        };

        // 111 * 1.08 = 119.88 -> Math.Floor = 119
        Assert.Equal(119m, item.LineTotalWithTax);
    }
}

public class MainViewModelTests
{
    private static MainViewModel CreateViewModel()
    {
        var vm = new MainViewModel();
        var field = typeof(MainViewModel).GetField("_dbContext",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(vm, new Moq.Mock<ConveniencePos.Data.PosDbContext>().Object);
        return vm;
    }

    private static void AddCartItem(MainViewModel vm, int productId, string name,
        decimal unitPrice, int taxRate, int quantity)
    {
        var item = new CartItemViewModel
        {
            ProductId = productId,
            Name = name,
            UnitPrice = unitPrice,
            TaxRate = taxRate,
            Quantity = quantity
        };
        vm.CartItems.Add(item);
    }

    [Fact]
    public void Subtotal_EmptyCart_ReturnsZero()
    {
        var vm = CreateViewModel();

        Assert.Equal(0m, vm.Subtotal);
    }

    [Fact]
    public void Subtotal_SingleItem_ReturnsCorrectValue()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 2);

        Assert.Equal(240m, vm.Subtotal);
    }

    [Fact]
    public void Subtotal_MultipleItems_ReturnsSum()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);
        AddCartItem(vm, 2, "お茶", 150m, 8, 1);
        AddCartItem(vm, 3, "ポテトチップス", 180m, 10, 1);

        Assert.Equal(450m, vm.Subtotal);
    }

    [Fact]
    public void TaxableAmount8_OnlyReducedRateItems()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);
        AddCartItem(vm, 2, "お茶", 150m, 8, 1);
        AddCartItem(vm, 3, "ポテトチップス", 180m, 10, 1);

        Assert.Equal(270m, vm.TaxableAmount8);
    }

    [Fact]
    public void TaxableAmount10_OnlyStandardRateItems()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);
        AddCartItem(vm, 3, "ポテトチップス", 180m, 10, 1);
        AddCartItem(vm, 4, "ティッシュ", 200m, 10, 1);

        Assert.Equal(380m, vm.TaxableAmount10);
    }

    [Fact]
    public void TaxAmount8_CalculatesFloorOf8Percent()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);
        AddCartItem(vm, 2, "お茶", 150m, 8, 1);

        // TaxableAmount8 = 270, 270 * 0.08 = 21.6 -> Math.Floor = 21
        Assert.Equal(21m, vm.TaxAmount8);
    }

    [Fact]
    public void TaxAmount10_CalculatesFloorOf10Percent()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 3, "ポテトチップス", 180m, 10, 1);
        AddCartItem(vm, 4, "ティッシュ", 200m, 10, 1);

        // TaxableAmount10 = 380, 380 * 0.10 = 38
        Assert.Equal(38m, vm.TaxAmount10);
    }

    [Fact]
    public void TaxAmount_Sums8And10Percent()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);
        AddCartItem(vm, 3, "ポテトチップス", 180m, 10, 1);

        // Tax8: 120 * 0.08 = 9.6 -> 9, Tax10: 180 * 0.10 = 18
        Assert.Equal(27m, vm.TaxAmount);
    }

    [Fact]
    public void TotalAmount_SumsSubtotalAndTax()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);
        AddCartItem(vm, 3, "ポテトチップス", 180m, 10, 1);

        // Subtotal = 300, Tax = 27
        Assert.Equal(327m, vm.TotalAmount);
    }

    [Fact]
    public void Change_CalculatesCorrectly()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);

        // Subtotal = 120, Tax = 9 (120*0.08=9.6->9), Total = 129
        vm.ReceivedAmount = 200m;

        Assert.Equal(71m, vm.Change);
    }

    [Fact]
    public void Change_InsufficientPayment_ReturnsZero()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);

        // Total = 129
        vm.ReceivedAmount = 100m;

        Assert.Equal(0m, vm.Change);
    }

    [Fact]
    public void Change_ZeroReceivedAmount_ReturnsZero()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);

        Assert.Equal(0m, vm.Change);
    }

    [Fact]
    public void TaxAmount_FloorTruncation_Applied()
    {
        var vm = CreateViewModel();
        // 111 * 0.08 = 8.88 -> Math.Floor = 8
        AddCartItem(vm, 1, "テスト商品", 111m, 8, 1);

        Assert.Equal(8m, vm.TaxAmount8);
    }

    [Fact]
    public void MixedTaxScenario_SeedProducts()
    {
        var vm = CreateViewModel();
        // Seed data: おにぎり(120, 8%), お茶(150, 8%), チップス(180, 10%), ティッシュ(200, 10%), コーヒー(110, 10%)
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);
        AddCartItem(vm, 2, "お茶 500ml", 150m, 8, 1);
        AddCartItem(vm, 3, "ポテトチップス", 180m, 10, 1);
        AddCartItem(vm, 4, "ティッシュ", 200m, 10, 1);
        AddCartItem(vm, 5, "ホットコーヒー 350ml", 110m, 10, 1);

        // Subtotal = 120 + 150 + 180 + 200 + 110 = 760
        Assert.Equal(760m, vm.Subtotal);

        // TaxableAmount8 = 120 + 150 = 270
        Assert.Equal(270m, vm.TaxableAmount8);

        // TaxableAmount10 = 180 + 200 + 110 = 490
        Assert.Equal(490m, vm.TaxableAmount10);

        // TaxAmount8 = Math.Floor(270 * 0.08) = Math.Floor(21.6) = 21
        Assert.Equal(21m, vm.TaxAmount8);

        // TaxAmount10 = Math.Floor(490 * 0.10) = Math.Floor(49.0) = 49
        Assert.Equal(49m, vm.TaxAmount10);

        // TaxAmount = 21 + 49 = 70
        Assert.Equal(70m, vm.TaxAmount);

        // TotalAmount = 760 + 70 = 830
        Assert.Equal(830m, vm.TotalAmount);
    }

    [Fact]
    public void ReceivedAmountChanged_RaisesPropertyChangedForChange()
    {
        var vm = CreateViewModel();
        AddCartItem(vm, 1, "おにぎり 梅", 120m, 8, 1);

        var changed = false;
        ((INotifyPropertyChanged)vm).PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.Change))
                changed = true;
        };

        vm.ReceivedAmount = 200m;

        Assert.True(changed);
    }

    [Fact]
    public void CartItems_IsEmptyByDefault()
    {
        var vm = CreateViewModel();

        Assert.Empty(vm.CartItems);
    }

    [Fact]
    public void BarcodeInput_DefaultIsEmpty()
    {
        var vm = CreateViewModel();

        Assert.Equal(string.Empty, vm.BarcodeInput);
    }

    [Fact]
    public void ReceivedAmount_DefaultIsZero()
    {
        var vm = CreateViewModel();

        Assert.Equal(0m, vm.ReceivedAmount);
    }
}
