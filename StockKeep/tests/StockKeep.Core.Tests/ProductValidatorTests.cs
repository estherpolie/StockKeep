namespace StockKeep.Core.Tests;

public class ProductValidatorTests
{
    private static Product Valid() => new()
    {
        Name = "USB-C Cable",
        Sku = "CBL-001",
        Quantity = 10,
        Price = 4.99m,
        LowStockThreshold = 3,
    };

    [Fact]
    public void Valid_product_has_no_errors()
    {
        Assert.Empty(ProductValidator.Validate(Valid()));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Name_is_required(string name)
    {
        var product = Valid();
        product.Name = name;

        Assert.Contains("Name is required.", ProductValidator.Validate(product));
    }

    [Fact]
    public void Name_longer_than_limit_is_rejected()
    {
        var product = Valid();
        product.Name = new string('a', ProductValidator.MaxNameLength + 1);

        Assert.Single(ProductValidator.Validate(product));
    }

    [Fact]
    public void Sku_is_required()
    {
        var product = Valid();
        product.Sku = "";

        Assert.Contains("SKU is required.", ProductValidator.Validate(product));
    }

    [Fact]
    public void Sku_with_inner_space_is_rejected()
    {
        var product = Valid();
        product.Sku = "CBL 001";

        Assert.Contains("SKU must not contain spaces.", ProductValidator.Validate(product));
    }

    [Fact]
    public void Negative_quantity_is_rejected()
    {
        var product = Valid();
        product.Quantity = -1;

        Assert.Contains("Quantity cannot be negative.", ProductValidator.Validate(product));
    }

    [Fact]
    public void Negative_price_is_rejected()
    {
        var product = Valid();
        product.Price = -0.01m;

        Assert.Contains("Price cannot be negative.", ProductValidator.Validate(product));
    }

    [Fact]
    public void Zero_quantity_and_price_are_allowed()
    {
        var product = Valid();
        product.Quantity = 0;
        product.Price = 0m;

        Assert.Empty(ProductValidator.Validate(product));
    }

    [Fact]
    public void All_errors_are_reported_together()
    {
        var product = new Product { Name = "", Sku = "", Quantity = -1, Price = -1m, LowStockThreshold = -1 };

        Assert.Equal(5, ProductValidator.Validate(product).Count);
    }

    [Theory]
    [InlineData(5, 5, true)]   // at the threshold counts as low
    [InlineData(4, 5, true)]
    [InlineData(6, 5, false)]
    public void IsLowStock_compares_quantity_to_threshold(int quantity, int threshold, bool expected)
    {
        var product = new Product { Quantity = quantity, LowStockThreshold = threshold };

        Assert.Equal(expected, product.IsLowStock);
    }
}
