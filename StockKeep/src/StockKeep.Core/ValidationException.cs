namespace StockKeep.Core;

/// <summary>
/// Thrown by <see cref="ProductService"/> when a product cannot be saved.
/// Carries every problem found so the UI can show them all at once.
/// </summary>
public class ValidationException : Exception
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationException(IReadOnlyList<string> errors)
        : base(string.Join(Environment.NewLine, errors))
    {
        Errors = errors;
    }
}
