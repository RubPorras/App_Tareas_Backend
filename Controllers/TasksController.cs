using Backend.DTO.Task;
using Backend.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Backend.Controllers;

/// <summary>
/// Gestiona las operaciones relacionadas con las tareas.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;
    private readonly IValidator<CreateTaskDto> _createValidator;
    private readonly IValidator<UpdateTaskDto> _updateValidator;

    public TasksController(ITaskService taskService, IValidator<CreateTaskDto> createValidator, IValidator<UpdateTaskDto> updateValidator)
    {
        _taskService = taskService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    /// <summary>
    /// Crea una nueva tarea para el usuario autenticado.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<GetTaskDto>> Create(
        CreateTaskDto request)
    {
        // Validamos los datos enviados por el cliente.
        var validationResult =
            await _createValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .ToArray());

            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        // Extraemos el ID del usuario desde el JWT.
        // Nunca utilizamos un UserId enviado por el cliente.
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            // El token fue autenticado, pero no contiene
            // un identificador de usuario válido.
            return Unauthorized();
        }

        var task = await _taskService.CreateAsync(
            request,
            userId);

        return CreatedAtAction(
            nameof(GetById),
            new { id = task.Id },
            task);
    }

    /// <summary>
    /// Obtiene una tarea por su identificador.
    /// </summary>
    [HttpGet("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetTaskDto>> GetById(int id)
    {
        var task = await _taskService.GetByIdAsync(id);

        if (task is null)
        {
            return NotFound();
        }

        // Obtenemos el ID del usuario autenticado desde el JWT.
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Los administradores pueden consultar cualquier tarea.
        if (User.IsInRole("Admin"))
        {
            return Ok(task);
        }

        // Un usuario normal solamente puede consultar
        // sus propias tareas.
        if (task.UserId != userId)
        {
            return Forbid();
        }

        return Ok(task);
    }
    /// <summary>
    /// Obtiene las tareas visibles para el usuario autenticado.
    /// Los administradores pueden consultar todas las tareas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<TaskSummaryDto>>> GetAll(
        [FromQuery] PaginationParams pagination)
    {
        // Normalizamos los parámetros enviados por el cliente.
        // El servidor nunca confía directamente en estos valores.
        var page = Math.Max(1, pagination.Page);

        var pageSize = Math.Clamp(
            pagination.PageSize,
            1,
            PaginationParams.MaxPageSize);

        var normalizedPagination = new PaginationParams
        {
            Page = page,
            PageSize = pageSize
        };

        // Obtenemos el ID del usuario autenticado desde el JWT.
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Comprobamos si el usuario tiene privilegios de Admin.
        var isAdmin = User.IsInRole("Admin");

        var result = await _taskService.GetAllAsync(
            userId,
            isAdmin,
            normalizedPagination);

        return Ok(result);
    }
    /// <summary>
    /// Actualiza una tarea existente.
    /// </summary>
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetTaskDto>> Update(int id, UpdateTaskDto request)
    {
        // Validamos los datos enviados por el cliente.
        var validationResult =
            await _updateValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            var errors = validationResult
                .Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .ToArray());

            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        // Extraemos el ID del usuario autenticado desde el JWT.
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Determinamos el rol a partir del JWT.
        var isAdmin = User.IsInRole("Admin");

        // El servicio comprueba existencia y permisos.
        var task = await _taskService.UpdateAsync(
            id,
            request,
            userId,
            isAdmin);

        if (task is null)
        {
            // Aquí debemos distinguir entre:
            // 1. tarea inexistente
            // 2. tarea existente pero perteneciente a otro usuario
            var existingTask = await _taskService.GetByIdAsync(id);

            if (existingTask is null)
            {
                return NotFound();
            }

            return Forbid();
        }

        return Ok(task);
    }
    /// <summary>
    /// Elimina una tarea existente.
    /// </summary>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        // Obtenemos el ID del usuario autenticado desde el JWT.
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        // Comprobamos si el usuario posee el rol Admin.
        var isAdmin = User.IsInRole("Admin");

        var deleted = await _taskService.DeleteAsync(
            id,
            userId,
            isAdmin);

        if (deleted)
        {
            return NoContent();
        }

        // Si la operación no se realizó, necesitamos distinguir
        // entre una tarea inexistente y una tarea sin permisos.
        var task = await _taskService.GetByIdAsync(id);

        if (task is null)
        {
            return NotFound();
        }

        return Forbid();
    }
}