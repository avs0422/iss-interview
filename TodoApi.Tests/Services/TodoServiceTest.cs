using Moq;
using TodoApi.Contracts;
using TodoApi.Contracts.Requests;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Repository;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests.Services;

public class TodoServiceTest
{
    private readonly Mock<ITodoRepository> _repositoryMock;
    private readonly TodoService _service;

    public TodoServiceTest()
    {
        _repositoryMock = new Mock<ITodoRepository>();
        _service = new TodoService(_repositoryMock.Object);
    }

    [Fact]
    public async Task CreateAsync_ReturnsCreatedTodo_WhenRequestIsValid()
    {
        var request = new CreateTodoRequest
        {
            Title = "Test Title",
            Description = "2 liters",
            IsCompleted = false
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo todo, CancellationToken _) =>
            {
                todo.Id = 1;
                todo.CreatedAt = DateTime.UtcNow;
                return todo;
            });

        var result = await _service.CreateTodoAsync(request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Test Title", result.Title);
        Assert.Equal("2 liters", result.Description);
        Assert.False(result.IsCompleted);
    }

    [Fact]
    public async Task CreateAsync_TrimsTitleAndDescription()
    {
        var request = new CreateTodoRequest
        {
            Title = "  Test Title  ",
            Description = "  2 liters  ",
            IsCompleted = false
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo todo, CancellationToken _) =>
            {
                todo.Id = 1;
                todo.CreatedAt = DateTime.UtcNow;
                return todo;
            });

        var result = await _service.CreateTodoAsync(request, CancellationToken.None);

        Assert.Equal("Test Title", result.Title);
        Assert.Equal("2 liters", result.Description);
    }

    [Fact]
    public async Task CreateAsync_PassesMappedTodoToRepository()
    {
        var request = new CreateTodoRequest
        {
            Title = "Task",
            Description = "Desc",
            IsCompleted = true
        };

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Todo
            {
                Id = 10,
                Title = "Task",
                Description = "Desc",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            });

        await _service.CreateTodoAsync(request, CancellationToken.None);

        _repositoryMock.Verify(r =>
            r.CreateAsync(
                It.Is<Todo>(t =>
                    t.Title == "Task" &&
                    t.Description == "Desc" &&
                    t.IsCompleted),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsTodosFromRepository()
    {
        var todos = new List<Todo>
        {
            new Todo
            {
                Id = 1,
                Title = "Task 1",
                Description = "Desc 1",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new Todo
            {
                Id = 2,
                Title = "Task 2",
                Description = "Desc 2",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        var result = await _service.GetAllTodosAsync(CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Equal("Task 1", result[0].Title);
        Assert.Equal("Task 2", result[1].Title);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenRepositoryHasNoData()
    {
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Todo>());

        var result = await _service.GetAllTodosAsync(CancellationToken.None);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTodo_WhenFound()
    {
        var todo = new Todo
        {
            Id = 5,
            Title = "Task 5",
            Description = "Desc 5",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

        var result = await _service.GetTodoByIdAsync(5, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(5, result!.Id);
        Assert.Equal("Task 5", result.Title);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var result = await _service.GetTodoByIdAsync(999, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsUpdatedTodo_WhenTodoExists()
    {
        var existingTodo = new Todo
        {
            Id = 1,
            Title = "Old title",
            Description = "Old desc",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };

        var request = new UpdateTodoRequest
        {
            Title = "New title",
            Description = "New desc",
            IsCompleted = true
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTodo);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateTodoAsync(1, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(1, result!.Id);
        Assert.Equal("New title", result.Title);
        Assert.Equal("New desc", result.Description);
        Assert.True(result.IsCompleted);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenTodoDoesNotExist()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Updated title",
            Description = "Updated desc",
            IsCompleted = true
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var result = await _service.UpdateTodoAsync(999, request, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_TrimsUpdatedFields()
    {
        var existingTodo = new Todo
        {
            Id = 1,
            Title = "Old title",
            Description = "Old desc",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var request = new UpdateTodoRequest
        {
            Title = "  Updated title  ",
            Description = "  Updated desc  ",
            IsCompleted = true
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTodo);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.UpdateTodoAsync(1, request, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("Updated title", result!.Title);
        Assert.Equal("Updated desc", result.Description);
    }

    [Fact]
    public async Task UpdateAsync_CallsRepositoryUpdate_WhenTodoExists()
    {
        var existingTodo = new Todo
        {
            Id = 2,
            Title = "Old",
            Description = "Old desc",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var request = new UpdateTodoRequest
        {
            Title = "Updated",
            Description = "Updated desc",
            IsCompleted = true
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTodo);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _service.UpdateTodoAsync(2, request, CancellationToken.None);

        _repositoryMock.Verify(r =>
            r.UpdateAsync(
                It.Is<Todo>(t =>
                    t.Id == 2 &&
                    t.Title == "Updated" &&
                    t.Description == "Updated desc" &&
                    t.IsCompleted),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_DoesNotCallUpdate_WhenTodoDoesNotExist()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Updated",
            Description = "Updated desc",
            IsCompleted = true
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(100, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var result = await _service.UpdateTodoAsync(100, request, CancellationToken.None);

        Assert.Null(result);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenRepositoryUpdateFails()
    {
        var existingTodo = new Todo
        {
            Id = 3,
            Title = "Old",
            Description = "Old desc",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        var request = new UpdateTodoRequest
        {
            Title = "Updated",
            Description = "Updated desc",
            IsCompleted = true
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingTodo);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Todo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.UpdateTodoAsync(3, request, CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenRepositoryDeletesTodo()
    {
        _repositoryMock
            .Setup(r => r.DeleteAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _service.DeleteTodoAsync(1, CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenRepositoryDoesNotDeleteTodo()
    {
        _repositoryMock
            .Setup(r => r.DeleteAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _service.DeleteTodoAsync(999, CancellationToken.None);

        Assert.False(result);
    }
}