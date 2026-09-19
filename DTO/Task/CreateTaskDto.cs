namespace Backend.DTO.Task;

/// <summary>
/// Datos necesarios para crear una nueva tarea.
/// </summary>
public sealed class CreateTaskDto
{
    // Título de la tarea.
    public string Title { get; init; } = string.Empty;

    // Descripción opcional.
    public string? Description { get; init; }
}