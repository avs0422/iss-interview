using TodoApi.Contracts.Requests;
using TodoApi.Contracts.Response;
using TodoApi.Models;

namespace TodoApi.Services;

public interface ITodoService
{
    Task<CreateTodoResponse> CreateTodoAsync(CreateTodoRequest request, CancellationToken cancellationToken = default);
    Task<List<Todo>> GetAllTodosAsync(CancellationToken cancellationToken = default);
    Task<Todo> GetTodoByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<UpdateTodoResponse> UpdateTodoAsync(int id, UpdateTodoRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteTodoAsync(int id, CancellationToken cancellationToken = default);
}