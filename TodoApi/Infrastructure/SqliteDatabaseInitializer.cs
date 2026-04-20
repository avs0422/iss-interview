using Microsoft.Data.Sqlite;
using TodoApi.Infrastructure.Interface;

namespace TodoApi.Infrastructure;

public sealed class SqliteDatabaseInitializer : IDatabaseInitializer
{
    private readonly string _connectionString;

    public SqliteDatabaseInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TodoDb")
                            ?? throw new InvalidOperationException("Connection string 'TodoDb' was not found.");
    }

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Todos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT,
                IsCompleted INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL
            )
        ";

        await command.ExecuteNonQueryAsync(cancellationToken);
    }
}