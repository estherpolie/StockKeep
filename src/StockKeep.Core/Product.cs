namespace StockKeep.Core;

/// <summary>
/// A single item in the inventory.
/// </summary>
public class Product
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Stock-keeping unit. Unique per product (case-insensitive).</summary>
    public string Sku { get; set; } = string.Empty;

    public int Quantity { get; set; }

    public decimal Price { get; set; }

    /// <summary>When Quantity is at or below this value the product is "low stock".</summary>
    public int LowStockThreshold { get; set; } = 5;

    public bool IsLowStock => Quantity <= LowStockThreshold;
}
