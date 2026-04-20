using Microsoft.AspNetCore.Mvc;
using Moq;
using TodoApi.Contracts;
using TodoApi.Contracts.Requests;
using TodoApi.Contracts.Response;
using TodoApi.Controllers;
using TodoApi.Models;
using TodoApi.Services;
using Xunit;

namespace TodoApi.Tests.Controllers;

public class TodoControllerTests
{
    private readonly Mock<ITodoService> _todoServiceMock;
    private readonly TodoController _controller;

    public TodoControllerTests()
    {
        _todoServiceMock = new Mock<ITodoService>();
        _controller = new TodoController(_todoServiceMock.Object);
    }

    [Fact]
    public async Task Create_ReturnsCreatedAtAction_WhenRequestIsValid()
    {
        var request = new CreateTodoRequest
        {
            Title = "Test Title",
            Description = "Test Description",
            IsCompleted = false
        };

        var createdTodo = new CreateTodoResponse()
        {
            Id = 1,
            Title = "Test Title",
            Description = "Test Description",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _todoServiceMock
            .Setup(s => s.CreateTodoAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdTodo);

        var result = await _controller.Create(request, CancellationToken.None);

        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(nameof(TodoController.GetTodoById), createdAtActionResult.ActionName);
        Assert.Equal(1, createdAtActionResult.RouteValues!["id"]);

        var returnedTodo = Assert.IsType<CreateTodoResponse>(createdAtActionResult.Value);
        Assert.Equal(createdTodo.Id, returnedTodo.Id);
        Assert.Equal(createdTodo.Title, returnedTodo.Title);
    }
    [Fact]
    public async Task Create_CallsServiceWithExpectedRequest()
    {
        var request = new CreateTodoRequest
        {
            Title = "Test",
            Description = "Desc",
            IsCompleted = false
        };

        _todoServiceMock
            .Setup(s => s.CreateTodoAsync(request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CreateTodoResponse{
                Id = 10,
                Title = request.Title,
                Description = request.Description,
                IsCompleted = request.IsCompleted,
                CreatedAt = DateTime.UtcNow
            });

        await _controller.Create(request, CancellationToken.None);

        _todoServiceMock.Verify(
            s => s.CreateTodoAsync(
                It.Is<CreateTodoRequest>(r =>
                    r.Title == request.Title &&
                    r.Description == request.Description &&
                    r.IsCompleted == request.IsCompleted),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithTodoList()
    {
        var todos = new List<Todo>
        {
            new Todo
            {
                Id = 1,
                Title = "Title 1",
                Description = "Desc 1",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow
            },
            new Todo
            {
                Id = 2,
                Title = "Title 2",
                Description = "Desc 2",
                IsCompleted = true,
                CreatedAt = DateTime.UtcNow
            }
        };

        _todoServiceMock
            .Setup(s => s.GetAllTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(todos);

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTodos = Assert.IsAssignableFrom<IReadOnlyList<Todo>>(okResult.Value);
        Assert.Equal(2, returnedTodos.Count);
    }

    [Fact]
    public async Task GetAll_ReturnsOk_WithEmptyList()
    {
        _todoServiceMock
            .Setup(s => s.GetAllTodosAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Todo>());

        var result = await _controller.GetAll(CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTodos = Assert.IsAssignableFrom<IReadOnlyList<Todo>>(okResult.Value);
        Assert.Empty(returnedTodos);
    }

    [Fact]
    public async Task GetById_ReturnsOk_WhenTodoExists()
    {
        var todo = new Todo
        {
            Id = 1,
            Title = "Title 1",
            Description = "Desc 1",
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };

        _todoServiceMock
            .Setup(s => s.GetTodoByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(todo);

        var result = await _controller.GetTodoById(1, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTodo = Assert.IsType<Todo>(okResult.Value);
        Assert.Equal(1, returnedTodo.Id);
    }

    [Fact]
    public async Task GetById_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        _todoServiceMock
            .Setup(s => s.GetTodoByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Todo?)null);

        var result = await _controller.GetTodoById(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_ReturnsOk_WhenTodoExistsAndRequestIsValid()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Updated task",
            Description = "Updated desc",
            IsCompleted = true
        };

        var updatedTodo = new UpdateTodoResponse()
        {
            Id = 1,
            Title = request.Title,
            Description = request.Description,
            IsCompleted = request.IsCompleted,
            CreatedAt = DateTime.UtcNow
        };

        _todoServiceMock
            .Setup(s => s.UpdateTodoAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(updatedTodo);

        var result = await _controller.Update(1, request, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTodo = Assert.IsType<UpdateTodoResponse>(okResult.Value);
        Assert.Equal(1, returnedTodo.Id);
        Assert.True(returnedTodo.IsCompleted);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Updated task",
            Description = "Updated desc",
            IsCompleted = true
        };

        _todoServiceMock
            .Setup(s => s.UpdateTodoAsync(1, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UpdateTodoResponse)null);

        var result = await _controller.Update(1, request, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public async Task Update_CallsServiceWithExpectedIdAndRequest()
    {
        var request = new UpdateTodoRequest
        {
            Title = "Updated title",
            Description = "Updated desc",
            IsCompleted = true
        };

        _todoServiceMock
            .Setup(s => s.UpdateTodoAsync(5, request, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UpdateTodoResponse()
            {
                Id = 5,
                Title = request.Title,
                Description = request.Description,
                IsCompleted = request.IsCompleted,
                CreatedAt = DateTime.UtcNow
            });

        await _controller.Update(5, request, CancellationToken.None);

        _todoServiceMock.Verify(
            s => s.UpdateTodoAsync(
                5,
                It.Is<UpdateTodoRequest>(r =>
                    r.Title == request.Title &&
                    r.Description == request.Description &&
                    r.IsCompleted == request.IsCompleted),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent_WhenTodoExists()
    {
        _todoServiceMock
            .Setup(s => s.DeleteTodoAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenTodoDoesNotExist()
    {
        _todoServiceMock
            .Setup(s => s.DeleteTodoAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _controller.Delete(1, CancellationToken.None);

        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task Delete_CallsServiceWithExpectedId()
    {
        _todoServiceMock
            .Setup(s => s.DeleteTodoAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await _controller.Delete(1, CancellationToken.None);

        _todoServiceMock.Verify(
            s => s.DeleteTodoAsync(1, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}