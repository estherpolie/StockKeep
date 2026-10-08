namespace StockKeep.Core;

/// <summary>
/// The business rules for products. The UI calls this; this calls the repository.
/// Keeping rules here (not in the form) means they are tested and can't be bypassed by the UI.
/// </summary>
public class ProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Product> List(string? searchTerm = null) =>
        string.IsNullOrWhiteSpace(searchTerm) ? _repository.GetAll() : _repository.Search(searchTerm);

    public Product? Get(int id) => _repository.GetById(id);

    /// <exception cref="ValidationException">If the product is invalid or its SKU is taken.</exception>
    public int Add(Product product)
    {
        EnsureValid(product, excludeId: null);
        return _repository.Add(product);
    }

    /// <exception cref="ValidationException">If the product is invalid or its SKU is taken.</exception>
    /// <exception cref="KeyNotFoundException">If the product no longer exists.</exception>
    public void Update(Product product)
    {
        EnsureValid(product, excludeId: product.Id);
        if (!_repository.Update(product))
            throw new KeyNotFoundException($"Product {product.Id} was not found.");
    }

    /// <returns>True if the product existed and was deleted.</returns>
    public bool Delete(int id) => _repository.Delete(id);

    public int CountLowStock(IEnumerable<Product> products) => products.Count(p => p.IsLowStock);

    private void EnsureValid(Product product, int? excludeId)
    {
        var errors = ProductValidator.Validate(product).ToList();

        if (errors.Count == 0 && _repository.SkuExists(product.Sku, excludeId))
            errors.Add($"SKU '{product.Sku.Trim()}' is already used by another product.");

        if (errors.Count > 0)
            throw new ValidationException(errors);
    }
}
