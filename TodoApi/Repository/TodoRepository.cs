using Microsoft.Data.Sqlite;
using TodoApi.Models;
using TodoApi.Repository;

namespace TodoApi.Repositories;

public class TodoRepository : ITodoRepository
{
    private readonly string _connectionString;

    public TodoRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("TodoDb") 
                            ?? throw new InvalidOperationException("TodoDb connection string not set");
    }

    public async Task<Todo> CreateAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        var createdAt = DateTime.UtcNow;

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Todos (Title, Description, IsCompleted, CreatedAt)
            VALUES (@title, @description, @isCompleted, @createdAt);
            SELECT last_insert_rowid();";
        command.Parameters.AddWithValue("@title", todo.Title);
        command.Parameters.AddWithValue("@description", (object?)todo.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@isCompleted", todo.IsCompleted ? 1 : 0);
        command.Parameters.AddWithValue("@createdAt", createdAt.ToString("O"));

        var id = Convert.ToInt32(await command.ExecuteScalarAsync(cancellationToken));

        todo.Id = id;
        todo.CreatedAt = createdAt;
        return todo;
    }

    public async Task<List<Todo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var todos = new List<Todo>();

        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Todos ORDER BY Id;";

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            todos.Add(Map(reader));
        }

        return todos;
    }

    public async Task<Todo> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "SELECT Id, Title, Description, IsCompleted, CreatedAt FROM Todos WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        return await reader.ReadAsync(cancellationToken) ? Map(reader) : null;
    }

    public async Task<bool> UpdateAsync(Todo todo, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = @"
            UPDATE Todos
            SET Title = @title,
                Description = @description,
                IsCompleted = @isCompleted
            WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", todo.Id);
        command.Parameters.AddWithValue("@title", todo.Title);
        command.Parameters.AddWithValue("@description", (object?)todo.Description ?? DBNull.Value);
        command.Parameters.AddWithValue("@isCompleted", todo.IsCompleted ? 1 : 0);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        await using var connection = new SqliteConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);

        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Todos WHERE Id = @id;";
        command.Parameters.AddWithValue("@id", id);

        var rowsAffected = await command.ExecuteNonQueryAsync(cancellationToken);
        return rowsAffected > 0;
    }

    private static Todo Map(SqliteDataReader reader) => new()
    {
        Id = reader.GetInt32(0),
        Title = reader.GetString(1),
        Description = reader.IsDBNull(2) ? null : reader.GetString(2),
        IsCompleted = reader.GetInt32(3) == 1,
        CreatedAt = DateTime.Parse(reader.GetString(4))
    };
}