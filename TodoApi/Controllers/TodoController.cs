using Microsoft.AspNetCore.Mvc;
using TodoApi.Contracts;
using TodoApi.Contracts.Requests;
using TodoApi.Models;
using TodoApi.Services;

namespace TodoApi.Controllers;

[ApiController]
[Route("api/todos")]
public class TodoController : ControllerBase
{
    private readonly ITodoService _todoService;

    public TodoController(ITodoService todoService)
    {
        _todoService = todoService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(Todo), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Todo>> Create([FromBody] CreateTodoRequest request, CancellationToken cancellationToken)
    {
        var created = await _todoService.CreateTodoAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetTodoById), new { id = created.Id }, created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<Todo>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<Todo>>> GetAll(CancellationToken cancellationToken)
    {
        var todos = await _todoService.GetAllTodosAsync(cancellationToken);
        return Ok(todos);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Todo>> GetTodoById(int id, CancellationToken cancellationToken)
    {
        var todo = await _todoService.GetTodoByIdAsync(id, cancellationToken);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Todo>> Update(int id, [FromBody] UpdateTodoRequest request, CancellationToken cancellationToken)
    {
        var updated = await _todoService.UpdateTodoAsync(id, request, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _todoService.DeleteTodoAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}