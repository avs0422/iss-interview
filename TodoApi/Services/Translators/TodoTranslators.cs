using TodoApi.Contracts.Requests;
using TodoApi.Contracts.Response;
using TodoApi.Models;

namespace TodoApi.Services.Translators;

public static class TodoTranslators
{
    public static Todo ToModel(this CreateTodoRequest request)
    {
        if (request == null) return null;
        return new Todo
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            IsCompleted = request.IsCompleted
        };
    }

    public static Todo ToModel(this UpdateTodoRequest request, int id, DateTime createdAt)
    {
        if (request == null) return null;
        return new Todo
        {
            Id = id,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            IsCompleted = request.IsCompleted,
            CreatedAt = createdAt
        };
    }

    public static CreateTodoResponse ToCreateResponse(this Todo todo)
    {
        if (todo == null) return null;
        return new CreateTodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt
        };
    }

    public static UpdateTodoResponse ToUpdateResponse(this Todo todo)
    {
        if (todo == null) return null;
        return new UpdateTodoResponse
        {
            Id = todo.Id,
            Title = todo.Title,
            Description = todo.Description,
            IsCompleted = todo.IsCompleted,
            CreatedAt = todo.CreatedAt
        };
    }
}