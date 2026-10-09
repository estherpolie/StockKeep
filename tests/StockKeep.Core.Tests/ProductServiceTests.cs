namespace StockKeep.Core.Tests;

/// <summary>
/// End-to-end tests of the business rules against a real (in-memory) SQLite database.
/// </summary>
public sealed class ProductServiceTests : IDisposable
{
    private readonly InMemoryDatabase _db = new();
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _service = new ProductService(_db.Repository);
    }

    public void Dispose() => _db.Dispose();

    private static Product NewProduct(string sku = "CBL-001", string name = "USB-C Cable", int quantity = 10) => new()
    {
        Name = name,
        Sku = sku,
        Quantity = quantity,
        Price = 4.99m,
        LowStockThreshold = 3,
    };

    [Fact]
    public void Add_then_Get_returns_the_saved_product()
    {
        var id = _service.Add(NewProduct());

        var saved = _service.Get(id);

        Assert.NotNull(saved);
        Assert.Equal("USB-C Cable", saved.Name);
        Assert.Equal("CBL-001", saved.Sku);
        Assert.Equal(10, saved.Quantity);
        Assert.Equal(4.99m, saved.Price);
        Assert.Equal(3, saved.LowStockThreshold);
    }

    [Fact]
    public void Add_trims_whitespace()
    {
        var id = _service.Add(NewProduct(sku: "  CBL-001 ", name: "  Cable  "));

        var saved = _service.Get(id)!;

        Assert.Equal("CBL-001", saved.Sku);
        Assert.Equal("Cable", saved.Name);
    }

    [Fact]
    public void Add_invalid_product_throws_and_saves_nothing()
    {
        var ex = Assert.Throws<ValidationException>(() => _service.Add(NewProduct(quantity: -5)));

        Assert.Contains("Quantity cannot be negative.", ex.Errors);
        Assert.Empty(_service.List());
    }

    [Theory]
    [InlineData("CBL-001")]
    [InlineData("cbl-001")] // SKUs are case-insensitive
    public void Add_duplicate_sku_is_rejected(string duplicateSku)
    {
        _service.Add(NewProduct(sku: "CBL-001"));

        var ex = Assert.Throws<ValidationException>(() => _service.Add(NewProduct(sku: duplicateSku)));

        Assert.Contains(ex.Errors, e => e.Contains("already used"));
    }

    [Fact]
    public void Update_changes_the_stored_values()
    {
        var id = _service.Add(NewProduct());
        var product = _service.Get(id)!;
        product.Quantity = 2;
        product.Price = 5.50m;

        _service.Update(product);

        var saved = _service.Get(id)!;
        Assert.Equal(2, saved.Quantity);
        Assert.Equal(5.50m, saved.Price);
        Assert.True(saved.IsLowStock);
    }

    [Fact]
    public void Update_keeping_own_sku_is_allowed()
    {
        var id = _service.Add(NewProduct());
        var product = _service.Get(id)!;
        product.Name = "Braided USB-C Cable";

        _service.Update(product); // must not complain that its own SKU is "taken"

        Assert.Equal("Braided USB-C Cable", _service.Get(id)!.Name);
    }

    [Fact]
    public void Update_to_another_products_sku_is_rejected()
    {
        _service.Add(NewProduct(sku: "CBL-001"));
        var secondId = _service.Add(NewProduct(sku: "CBL-002"));
        var second = _service.Get(secondId)!;
        second.Sku = "CBL-001";

        Assert.Throws<ValidationException>(() => _service.Update(second));
    }

    [Fact]
    public void Update_missing_product_throws()
    {
        var ghost = NewProduct();
        ghost.Id = 999;

        Assert.Throws<KeyNotFoundException>(() => _service.Update(ghost));
    }

    [Fact]
    public void Delete_removes_the_product()
    {
        var id = _service.Add(NewProduct());

        Assert.True(_service.Delete(id));
        Assert.Null(_service.Get(id));
        Assert.False(_service.Delete(id)); // second delete finds nothing
    }

    [Fact]
    public void List_with_search_matches_name_or_sku_case_insensitively()
    {
        _service.Add(NewProduct(sku: "CBL-001", name: "USB-C Cable"));
        _service.Add(NewProduct(sku: "MSE-001", name: "Wireless Mouse"));

        Assert.Single(_service.List("mouse"));
        Assert.Single(_service.List("cbl"));
        Assert.Equal(2, _service.List("").Count);
        Assert.Empty(_service.List("keyboard"));
    }

    [Fact]
    public void AdjustStock_adds_received_items()
    {
        var id = _service.Add(NewProduct(quantity: 10));

        _service.AdjustStock(id, 5);

        Assert.Equal(15, _service.Get(id)!.Quantity);
    }

    [Fact]
    public void AdjustStock_removes_sold_items()
    {
        var id = _service.Add(NewProduct(quantity: 10));

        _service.AdjustStock(id, -4);

        Assert.Equal(6, _service.Get(id)!.Quantity);
    }

    [Fact]
    public void List_is_sorted_by_name()
    {
        _service.Add(NewProduct(sku: "B", name: "banana"));
        _service.Add(NewProduct(sku: "A", name: "Apple"));

        var names = _service.List().Select(p => p.Name).ToList();

        Assert.Equal(new[] { "Apple", "banana" }, names);
    }

    [Fact]
    public void CountLowStock_counts_only_low_items()
    {
        _service.Add(NewProduct(sku: "A", quantity: 1));
        _service.Add(NewProduct(sku: "B", quantity: 50));

        Assert.Equal(1, _service.CountLowStock(_service.List()));
    }
}
