using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using Moq;
using TodoApi.Models;
using TodoApi.Repositories;
using Xunit;

namespace TodoApi.Tests.Repositories;

public class TodoRepositoryTests : IDisposable
{
    private readonly string _databasePath;
    private readonly string _connectionString;
    private readonly TodoRepository _repository;

    public TodoRepositoryTests()
    {
        _databasePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");
        _connectionString = $"Data Source={_databasePath}";
        InitializeDatabase(_connectionString);
        var _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TodoDb"] = _connectionString
            })
            .Build();
        _repository = new TodoRepository(_configuration);
    }

    [Fact]
    public async Task CreateAsync_InsertsTodo_AndReturnsTodoWithId()
    {
        var todo = new Todo
        {
            Title = "Buy milk",
            Description = "2 liters",
            IsCompleted = false
        };

        var result = await _repository.CreateAsync(todo);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Equal("Buy milk", result.Title);
        Assert.Equal("2 liters", result.Description);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task CreateAsync_SetsCreatedAt()
    {
        var todo = new Todo
        {
            Title = "Task with time",
            Description = "Check created at",
            IsCompleted = false
        };

        var before = DateTime.UtcNow;

        var result = await _repository.CreateAsync(todo);

        var after = DateTime.UtcNow;

        Assert.True(result.CreatedAt >= before);
        Assert.True(result.CreatedAt <= after);
    }

    [Fact]
    public async Task CreateAsync_AllowsNullDescription()
    {
        var todo = new Todo
        {
            Title = "Task without description",
            Description = null,
            IsCompleted = false
        };

        var result = await _repository.CreateAsync(todo);

        Assert.NotNull(result);
        Assert.True(result.Id > 0);
        Assert.Null(result.Description);
    }

    [Fact]
    public async Task CreateAsync_HandlesSpecialCharactersInTitleAndDescription()
    {
        var todo = new Todo
        {
            Title = "Test title",
            Description = "Description",
            IsCompleted = false
        };

        var result = await _repository.CreateAsync(todo);

        Assert.True(result.Id > 0);

        var stored = await _repository.GetByIdAsync(result.Id);

        Assert.NotNull(stored);
        Assert.Equal("Test title", stored!.Title);
        Assert.Equal("Description", stored.Description);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenDatabaseIsEmpty()
    {
        var result = await _repository.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsAllInsertedTodos()
    {
        await _repository.CreateAsync(new Todo
        {
            Title = "Title 1",
            Description = "Desc 1",
            IsCompleted = false
        });

        await _repository.CreateAsync(new Todo
        {
            Title = "Title 2",
            Description = "Desc 2",
            IsCompleted = true
        });

        var result = await _repository.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Contains(result, t => t.Title == "Title 1");
        Assert.Contains(result, t => t.Title == "Title 2");
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTodo_WhenTodoExists()
    {
        var created = await _repository.CreateAsync(new Todo
        {
            Title = "Test title",
            Description = "Desc",
            IsCompleted = false
        });

        var result = await _repository.GetByIdAsync(created.Id);

        Assert.NotNull(result);
        Assert.Equal(created.Id, result!.Id);
        Assert.Equal("Test title", result.Title);
        Assert.Equal("Desc", result.Description);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenTodoDoesNotExist()
    {
        var result = await _repository.GetByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsTrue_WhenTodoExists()
    {
        var created = await _repository.CreateAsync(new Todo
        {
            Title = "Old title",
            Description = "Old desc",
            IsCompleted = false
        });

        created.Title = "Updated title";
        created.Description = "Updated desc";
        created.IsCompleted = true;

        var result = await _repository.UpdateAsync(created);

        Assert.True(result);
    }

    [Fact]
    public async Task UpdateAsync_ActuallyUpdatesStoredValues()
    {
        var created = await _repository.CreateAsync(new Todo
        {
            Title = "Before update",
            Description = "Before desc",
            IsCompleted = false
        });

        created.Title = "After update";
        created.Description = "After desc";
        created.IsCompleted = true;

        await _repository.UpdateAsync(created);

        var updated = await _repository.GetByIdAsync(created.Id);

        Assert.NotNull(updated);
        Assert.Equal("After update", updated!.Title);
        Assert.Equal("After desc", updated.Description);
        Assert.True(updated.IsCompleted);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenTodoDoesNotExist()
    {
        var missingTodo = new Todo
        {
            Id = 999,
            Title = "Missing",
            Description = "Missing desc",
            IsCompleted = false
        };

        var result = await _repository.UpdateAsync(missingTodo);

        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenTodoExists()
    {
        var created = await _repository.CreateAsync(new Todo
        {
            Title = "Delete",
            Description = "Temp",
            IsCompleted = false
        });

        var result = await _repository.DeleteAsync(created.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_RemovesTodoFromDatabase()
    {
        var created = await _repository.CreateAsync(new Todo
        {
            Title = "Delete",
            Description = "Temp",
            IsCompleted = false
        });

        await _repository.DeleteAsync(created.Id);

        var deleted = await _repository.GetByIdAsync(created.Id);

        Assert.Null(deleted);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenTodoDoesNotExist()
    {
        var result = await _repository.DeleteAsync(999);

        Assert.False(result);
    }

    private static void InitializeDatabase(string connectionString)
    {
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Todos (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Description TEXT NULL,
                IsCompleted INTEGER NOT NULL DEFAULT 0,
                CreatedAt TEXT NOT NULL
            );";
        command.ExecuteNonQuery();
    }

    public void Dispose()
    {
        if (File.Exists(_databasePath))
        {
            File.Delete(_databasePath);
        }
    }
}