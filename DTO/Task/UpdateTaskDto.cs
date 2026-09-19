namespace Backend.DTO.Task;

/// <summary>
/// Datos que pueden modificarse de una tarea existente.
/// </summary>
public sealed class UpdateTaskDto
{
    // Nuevo título de la tarea.
    public string Title { get; init; } = string.Empty;

    // Nueva descripción de la tarea.
    public string? Description { get; init; }

    // Indica si la tarea está completada.
    public bool IsCompleted { get; init; }
}