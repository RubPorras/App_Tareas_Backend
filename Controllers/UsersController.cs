using Backend.DTO.User;
using Backend.Service.UserService;
using Microsoft.AspNetCore.Mvc;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Backend.DTO.Task;

namespace Backend.Controllers;

/// <summary>
/// Controlador encargado de gestionar los usuarios.
/// </summary>
[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IValidator<CreateUserDto> _createUserValidator;
    private readonly IValidator<UpdateUserDto> _updateUserValidator;

    /// <summary>
    /// Constructor del controlador.
    /// </summary>
    public UsersController(IUserService userService, IValidator<CreateUserDto> createUserValidator, IValidator<UpdateUserDto> updateUserValidator)
    {
        _userService = userService;
        _createUserValidator = createUserValidator;
        _updateUserValidator = updateUserValidator;
    }

    /// <summary>
    /// Obtiene todos los usuarios.
    /// </summary>
    /// <returns>Lista de usuarios.</returns>
    [Authorize]
    [HttpGet]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    public async Task<ActionResult<PagedResult<GetUserDto>>> GetAll(
    [FromQuery] PaginationParams pagination)
    {
        var normalizedPagination = NormalizePagination(pagination);

        var users = await _userService.GetAllAsync(
            normalizedPagination);

        return Ok(users);
    }


    private static PaginationParams NormalizePagination(
        PaginationParams pagination)
    {
        var page = Math.Max(pagination.Page, 1);

        var pageSize = Math.Clamp(
            pagination.PageSize,
            1,
            PaginationParams.MaxPageSize);

        return new PaginationParams
        {
            Page = page,
            PageSize = pageSize
        };
    }

    /// <summary>
    /// Obtiene un usuario mediante su identificador.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <returns>Información del usuario.</returns>
    [Authorize]
    [HttpGet("{id:int}")]
    [ProducesResponseType(
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<ActionResult<GetUserDto>> GetById(int id)
    {
        var user = await _userService.GetByIdAsync(id);

        if (user is null)
        {
            return NotFound();
        }

        return Ok(user);
    }

    /// <summary>
    /// Registra un nuevo usuario.
    /// </summary>
    /// <param name="request">Datos necesarios para crear el usuario.</param>
    /// <returns>Usuario creado.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetUserDto>> Create(
        CreateUserDto request)
    {
        // Validamos los datos recibidos antes de ejecutar
        // cualquier operación relacionada con el usuario.
        var validationResult = await _createUserValidator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            // Convertimos los errores de FluentValidation al formato
            // estándar ProblemDetails de ASP.NET Core.
            var errors = validationResult
                .Errors
                .GroupBy(error => error.PropertyName)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .Select(error => error.ErrorMessage)
                        .ToArray());
            //return ValidationProblem(errors);
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        try
        {
            var user = await _userService.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = user.Id },
                user);
        }
        catch (InvalidOperationException)
        {
            // Username o Email ya registrado.
            return Conflict(new
            {
                message = "El nombre de usuario o correo electrónico ya está registrado."
            });
        }
    }

    /// <summary>
    /// Actualiza la información modificable de un usuario.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="request">Datos que serán actualizados.</param>
    /// <returns>Usuario actualizado.</returns>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<GetUserDto>> Update(
        int id,
        UpdateUserDto request)
    {
        // Validamos primero los datos enviados por el cliente.
        var validationResult =
            await _updateUserValidator.ValidateAsync(request);

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

        try
        {
            var user = await _userService.UpdateAsync(id, request);

            if (user is null)
            {
                return NotFound();
            }

            return Ok(user);
        }
        catch (InvalidOperationException)
        {
            // Otro usuario ya utiliza el Username o Email.
            return Conflict(new
            {
                message =
                    "El nombre de usuario o correo electrónico ya está registrado."
            });
        }
    }
    
    /// <summary>
    /// Elimina un usuario mediante su identificador.
    /// </summary>
    /// <param name="id">Identificador del usuario.</param>
    /// <returns>Resultado de la operación.</returns>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _userService.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            // El servicio utiliza esta excepción cuando la operación
            // violaría una regla de integridad del sistema.
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }

    /// <summary>
    /// Promueve un usuario al rol de administrador.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/promote")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Promote(int id)
    {
        var promoted = await _userService.PromoteAsync(id);

        if (!promoted)
        {
            return NotFound();
        }

        return NoContent();
    }

    // Degrada un administrador al rol de usuario.
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/demote")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Demote(int id)
    {
        try
        {
            var demoted = await _userService.DemoteAsync(id);

            if (!demoted)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }

    // Activa o desactiva un usuario.
    [Authorize(Roles = "Admin")]
    [HttpPost("{id:int}/toggle-active")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ToggleActive(int id)
    {
        var userIdClaim = User.FindFirst(
            System.Security.Claims.ClaimTypes.NameIdentifier);

        if (userIdClaim is null)
        {
            return Unauthorized();
        }

        var requestingUserId = int.Parse(userIdClaim.Value);

        try
        {
            var toggled = await _userService.ToggleActiveAsync(
                id,
                requestingUserId);

            if (!toggled)
            {
                return NotFound();
            }

            return NoContent();
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new
            {
                message = exception.Message
            });
        }
    }
}