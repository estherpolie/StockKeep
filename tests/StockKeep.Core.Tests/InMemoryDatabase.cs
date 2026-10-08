using Microsoft.Data.Sqlite;
using StockKeep.Core;

namespace StockKeep.Core.Tests;

/// <summary>
/// A fresh, private SQLite database that lives only in memory for one test.
/// SQLite deletes an in-memory database when its last connection closes, so we keep
/// one connection open for the lifetime of the test.
/// </summary>
public sealed class InMemoryDatabase : IDisposable
{
    private readonly SqliteConnection _keepAlive;

    public SqliteProductRepository Repository { get; }

    public InMemoryDatabase()
    {
        var connectionString = new SqliteConnectionStringBuilder
        {
            DataSource = $"stockkeep-test-{Guid.NewGuid():N}",
            Mode = SqliteOpenMode.Memory,
            Cache = SqliteCacheMode.Shared,
        }.ToString();

        _keepAlive = new SqliteConnection(connectionString);
        _keepAlive.Open();
        Repository = new SqliteProductRepository(connectionString);
    }

    public void Dispose() => _keepAlive.Dispose();
}
