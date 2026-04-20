using TodoApi.Contracts;
using TodoApi.Contracts.Requests;
using TodoApi.Contracts.Response;
using TodoApi.Models;
using TodoApi.Repositories;
using TodoApi.Repository;
using TodoApi.Services.Translators;

namespace TodoApi.Services;

public class TodoService : ITodoService
{
    private readonly ITodoRepository _repository;

    public TodoService(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<Todo>> GetAllTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken);
    }

    public async Task<Todo> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    public async Task<CreateTodoResponse> CreateTodoAsync(CreateTodoRequest request, CancellationToken cancellationToken = default)
    {
        var todo = request.ToModel();

        var result =  await _repository.CreateAsync(todo, cancellationToken);
        return result.ToCreateResponse();
    }

    public async Task<UpdateTodoResponse> UpdateTodoAsync(int id, UpdateTodoRequest request, CancellationToken cancellationToken = default)
    {
        var existing = await _repository.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return null;
        }
        existing = request.ToModel(id, DateTime.UtcNow);
        var updated = await _repository.UpdateAsync(existing, cancellationToken);
        return updated ? existing.ToUpdateResponse() : null;
    }

    public async Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken);
    }
}