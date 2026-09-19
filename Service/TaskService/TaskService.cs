using Backend.Data;
using Backend.DTO.Task;
using Backend.Model;
using Microsoft.EntityFrameworkCore;

namespace Backend.Services;

/// <summary>
/// Contiene la lógica de negocio relacionada con las tareas.
/// </summary>
public sealed class TaskService : ITaskService
{
    private readonly AppDbContext _context;

    public TaskService(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Crea una tarea asociada al usuario autenticado.
    /// </summary>
    public async Task<GetTaskDto> CreateAsync(
        CreateTaskDto dto,
        int userId)
    {
        var task = new TaskItem
        {
            // Normalizamos espacios innecesarios al principio y al final.
            Title = dto.Title.Trim(),

            // Si existe descripción, también eliminamos
            // espacios innecesarios en los extremos.
            Description = dto.Description?.Trim(),

            // Una tarea nueva siempre comienza como pendiente.
            IsCompleted = false,

            // El propietario viene del JWT, no del cliente.
            UserId = userId,

            CreatedAt = DateTime.UtcNow
        };

        _context.Tasks.Add(task);

        await _context.SaveChangesAsync();

        // Devolvemos un DTO y nunca exponemos directamente
        // nuestra entidad de EF Core.
        return new GetTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            UserId = task.UserId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }

    /// <summary>
    /// Obtiene una tarea por su identificador.
    /// </summary>
    public async Task<GetTaskDto?> GetByIdAsync(int id)
    {
        var task = await _context.Tasks
            .AsNoTracking()
            .FirstOrDefaultAsync(task => task.Id == id);

        if (task is null)
        {
            return null;
        }

        // Convertimos la entidad a DTO para no exponer
        // directamente nuestro modelo de EF Core.
        return new GetTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            UserId = task.UserId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
    /// <summary>
    /// Obtiene las tareas visibles para el usuario autenticado
    /// aplicando paginación directamente sobre la consulta SQL.
    /// </summary>
    public async Task<PagedResult<TaskSummaryDto>> GetAllAsync(
        int userId,
        bool isAdmin,
        PaginationParams pagination)
    {
        // Comenzamos con una consulta de solo lectura.
        var query = _context.Tasks
            .AsNoTracking();

        // Un usuario normal solamente puede consultar
        // sus propias tareas.
        if (!isAdmin)
        {
            query = query.Where(task => task.UserId == userId);
        }

        // Obtenemos la cantidad total de registros antes
        // de aplicar Skip/Take.
        var totalCount = await query.CountAsync();

        // Obtenemos únicamente los registros correspondientes
        // a la página solicitada.
        var items = await query
            .OrderByDescending(task => task.CreatedAt)
            .Skip(pagination.Skip)
            .Take(pagination.PageSize)
            .Select(task => new TaskSummaryDto
            {
                Id = task.Id,
                Title = task.Title,
                IsCompleted = task.IsCompleted
            })
            .ToListAsync();

        return new PagedResult<TaskSummaryDto>
        {
            Items = items,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalCount = totalCount
        };
    }
    
    /// <summary>
    /// Actualiza una tarea existente si el usuario tiene permisos.
    /// </summary>
    public async Task<GetTaskDto?> UpdateAsync(int id, UpdateTaskDto dto, int userId, bool isAdmin)
    {
        // Buscamos la tarea que se desea modificar.
        var task = await _context.Tasks
            .FirstOrDefaultAsync(task => task.Id == id);

        // La tarea no existe.
        if (task is null)
        {
            return null;
        }

        // Un usuario normal solamente puede modificar sus propias tareas.
        if (!isAdmin && task.UserId != userId)
        {
            return null;
        }

        // Actualizamos únicamente las propiedades que el cliente tiene permitido modificar.
        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.IsCompleted = dto.IsCompleted;

        // El servidor controla la fecha de modificación.
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Devolvemos un DTO, nunca la entidad de EF Core.
        return new GetTaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            IsCompleted = task.IsCompleted,
            UserId = task.UserId,
            CreatedAt = task.CreatedAt,
            UpdatedAt = task.UpdatedAt
        };
    }
    /// <summary>
    /// Elimina una tarea existente.
    /// </summary>
    public async Task<bool> DeleteAsync(int id, int userId, bool isAdmin)
    {
        // Buscamos la tarea que se desea eliminar.
        var task = await _context.Tasks.FirstOrDefaultAsync(task => task.Id == id);

        // La tarea no existe.
        if (task is null)
        {
            return false;
        }

        // Un usuario normal solamente puede eliminar
        // sus propias tareas.
        if (!isAdmin && task.UserId != userId)
        {
            return false;
        }

        _context.Tasks.Remove(task);

        await _context.SaveChangesAsync();

        return true;
    }
}