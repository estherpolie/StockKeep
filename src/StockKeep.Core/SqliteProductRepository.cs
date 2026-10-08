using Microsoft.Data.Sqlite;

namespace StockKeep.Core;

/// <summary>
/// Stores products in a SQLite database file.
/// Every query uses parameters ($name, $sku, ...) — never string concatenation — to prevent SQL injection.
/// </summary>
public class SqliteProductRepository : IProductRepository
{
    private readonly string _connectionString;

    public SqliteProductRepository(string connectionString)
    {
        _connectionString = connectionString;
        EnsureCreated();
    }

    /// <summary>Convenience constructor for a database file on disk.</summary>
    public static SqliteProductRepository ForFile(string path) =>
        new(new SqliteConnectionStringBuilder { DataSource = path }.ToString());

    private SqliteConnection Open()
    {
        var connection = new SqliteConnection(_connectionString);
        connection.Open();
        return connection;
    }

    private void EnsureCreated()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE IF NOT EXISTS Products (
                Id                INTEGER PRIMARY KEY AUTOINCREMENT,
                Name              TEXT    NOT NULL,
                Sku               TEXT    NOT NULL UNIQUE COLLATE NOCASE,
                Quantity          INTEGER NOT NULL,
                Price             TEXT    NOT NULL,
                LowStockThreshold INTEGER NOT NULL
            );
            """;
        command.ExecuteNonQuery();
    }

    public IReadOnlyList<Product> GetAll()
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Products ORDER BY Name COLLATE NOCASE;";
        return ReadAll(command);
    }

    public IReadOnlyList<Product> Search(string term)
    {
        if (string.IsNullOrWhiteSpace(term))
            return GetAll();

        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT * FROM Products
            WHERE Name LIKE $pattern ESCAPE '\' OR Sku LIKE $pattern ESCAPE '\'
            ORDER BY Name COLLATE NOCASE;
            """;
        command.Parameters.AddWithValue("$pattern", $"%{EscapeLike(term.Trim())}%");
        return ReadAll(command);
    }

    /// <summary>
    /// Makes % and _ match literally instead of acting as LIKE wildcards.
    /// </summary>
    private static string EscapeLike(string value) =>
        value.Replace("%", "\\%").Replace("_", "\\_");

    public Product? GetById(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT * FROM Products WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return ReadAll(command).FirstOrDefault();
    }

    public int Add(Product product)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Products (Name, Sku, Quantity, Price, LowStockThreshold)
            VALUES ($name, $sku, $quantity, $price, $threshold);
            SELECT last_insert_rowid();
            """;
        AddParameters(command, product);
        var id = Convert.ToInt32(command.ExecuteScalar());
        product.Id = id;
        return id;
    }

    public bool Update(Product product)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            UPDATE Products
            SET Name = $name, Sku = $sku, Quantity = $quantity,
                Price = $price, LowStockThreshold = $threshold
            WHERE Id = $id;
            """;
        AddParameters(command, product);
        command.Parameters.AddWithValue("$id", product.Id);
        return command.ExecuteNonQuery() == 1;
    }

    public bool Delete(int id)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Products WHERE Id = $id;";
        command.Parameters.AddWithValue("$id", id);
        return command.ExecuteNonQuery() == 1;
    }

    public bool SkuExists(string sku, int? excludeId = null)
    {
        using var connection = Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COUNT(*) FROM Products
            WHERE Sku = $sku COLLATE NOCASE AND ($excludeId IS NULL OR Id <> $excludeId);
            """;
        command.Parameters.AddWithValue("$sku", sku.Trim());
        command.Parameters.AddWithValue("$excludeId", (object?)excludeId ?? DBNull.Value);
        return Convert.ToInt64(command.ExecuteScalar()) > 0;
    }

    private static void AddParameters(SqliteCommand command, Product product)
    {
        command.Parameters.AddWithValue("$name", product.Name.Trim());
        command.Parameters.AddWithValue("$sku", product.Sku.Trim());
        command.Parameters.AddWithValue("$quantity", product.Quantity);
        // Stored as invariant text so decimal precision is never lost (SQLite REAL is a double).
        command.Parameters.AddWithValue("$price", product.Price.ToString(System.Globalization.CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$threshold", product.LowStockThreshold);
    }

    private static List<Product> ReadAll(SqliteCommand command)
    {
        var products = new List<Product>();
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            products.Add(new Product
            {
                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                Name = reader.GetString(reader.GetOrdinal("Name")),
                Sku = reader.GetString(reader.GetOrdinal("Sku")),
                Quantity = reader.GetInt32(reader.GetOrdinal("Quantity")),
                Price = decimal.Parse(reader.GetString(reader.GetOrdinal("Price")), System.Globalization.CultureInfo.InvariantCulture),
                LowStockThreshold = reader.GetInt32(reader.GetOrdinal("LowStockThreshold")),
            });
        }
        return products;
    }
}
