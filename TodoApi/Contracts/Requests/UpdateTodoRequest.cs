using System.ComponentModel.DataAnnotations;

namespace TodoApi.Contracts.Requests;

public class UpdateTodoRequest
{
    [Required]
    public string Title { get; set; }
    public string Description { get; set; }
    public bool IsCompleted { get; set; }
}