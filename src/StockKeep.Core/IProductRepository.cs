namespace StockKeep.Core;

/// <summary>
/// Storage for products. The app talks to this interface, not to SQLite directly,
/// so storage can be swapped or faked in tests.
/// </summary>
public interface IProductRepository
{
    IReadOnlyList<Product> GetAll();

    /// <summary>Products whose name or SKU contains <paramref name="term"/> (case-insensitive).</summary>
    IReadOnlyList<Product> Search(string term);

    Product? GetById(int id);

    /// <returns>The new product's Id.</returns>
    int Add(Product product);

    /// <returns>True if a row was updated.</returns>
    bool Update(Product product);

    /// <returns>True if a row was deleted.</returns>
    bool Delete(int id);

    /// <summary>True if another product already uses this SKU.</summary>
    /// <param name="excludeId">Ignore this product (used when editing).</param>
    bool SkuExists(string sku, int? excludeId = null);
}
