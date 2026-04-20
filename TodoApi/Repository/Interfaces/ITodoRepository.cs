using TodoApi.Models;

namespace TodoApi.Repository;

public interface ITodoRepository
{
    Task<Todo> CreateAsync(Todo todo, CancellationToken cancellationToken = default);
    Task<List<Todo>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Todo> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Todo todo, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
}