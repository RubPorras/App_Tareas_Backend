namespace Backend.DTO.Task;

/// <summary>
/// Representación resumida de una tarea.
/// Útil para listados donde no necesitamos todos los datos.
/// </summary>
public sealed class TaskSummaryDto
{
    // Identificador de la tarea.
    public int Id { get; init; }

    // Título de la tarea.
    public string Title { get; init; } = string.Empty;

    // Estado de la tarea.
    public bool IsCompleted { get; init; }
}