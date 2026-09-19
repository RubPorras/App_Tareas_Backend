using Backend.DTO.Task;

namespace Backend.Services;

/// <summary>
/// Define las operaciones disponibles para gestionar tareas.
/// </summary>
public interface ITaskService
{
    // Crea una nueva tarea para el usuario autenticado.
    Task<GetTaskDto> CreateAsync(CreateTaskDto dto, int userId);
    // Obtiene una tarea por su ID, asegurándose de que pertenezca al usuario autenticado.
    Task<GetTaskDto?> GetByIdAsync(int id);
    // Obtiene las tareas visibles para un usuario de forma paginada.
    Task<PagedResult<TaskSummaryDto>> GetAllAsync(int userId, bool isAdmin, PaginationParams pagination);
    // Actualiza una tarea existente.
    Task<GetTaskDto?> UpdateAsync(int id, UpdateTaskDto dto, int userId, bool isAdmin);
    // Elimina una tarea si el usuario tiene permisos.
    Task<bool> DeleteAsync(int id, int userId, bool isAdmin);
}