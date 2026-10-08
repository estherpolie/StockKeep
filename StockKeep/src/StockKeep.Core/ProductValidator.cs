namespace StockKeep.Core;

/// <summary>
/// Checks a product's fields. Pure logic, no database access, so it is easy to unit test.
/// </summary>
public static class ProductValidator
{
    public const int MaxNameLength = 100;
    public const int MaxSkuLength = 32;

    public static IReadOnlyList<string> Validate(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(product.Name))
            errors.Add("Name is required.");
        else if (product.Name.Trim().Length > MaxNameLength)
            errors.Add($"Name must be {MaxNameLength} characters or fewer.");

        if (string.IsNullOrWhiteSpace(product.Sku))
            errors.Add("SKU is required.");
        else if (product.Sku.Trim().Length > MaxSkuLength)
            errors.Add($"SKU must be {MaxSkuLength} characters or fewer.");
        else if (product.Sku.Trim().Any(char.IsWhiteSpace))
            errors.Add("SKU must not contain spaces.");

        if (product.Quantity < 0)
            errors.Add("Quantity cannot be negative.");

        if (product.Price < 0)
            errors.Add("Price cannot be negative.");

        if (product.LowStockThreshold < 0)
            errors.Add("Low-stock threshold cannot be negative.");

        return errors;
    }
}
