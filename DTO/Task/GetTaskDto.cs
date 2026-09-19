namespace Backend.DTO.Task;

/// <summary>
/// Representación completa de una tarea para respuestas de la API.
/// </summary>
public sealed class GetTaskDto
{
    // Identificador único de la tarea.
    public int Id { get; init; }

    // Título de la tarea.
    public string Title { get; init; } = string.Empty;

    // Descripción de la tarea.
    public string? Description { get; init; }

    // Estado actual de la tarea.
    public bool IsCompleted { get; init; }

    // Identificador del propietario.
    public int UserId { get; init; }

    // Fecha de creación.
    public DateTime CreatedAt { get; init; }

    // Fecha de última modificación.
    public DateTime? UpdatedAt { get; init; }
}