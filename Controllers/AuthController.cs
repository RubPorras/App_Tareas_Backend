using Backend.DTO.Auth;
using Backend.DTOs.Auth;
using Backend.Service.AuthService;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

/// <summary>
/// Gestiona las operaciones relacionadas con autenticación.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IValidator<LoginDto> _loginValidator;

    /// <summary>
    /// Constructor del controlador.
    /// </summary>
    public AuthController(
        IAuthService authService,
        IValidator<LoginDto> loginValidator)
    {
        _authService = authService;
        _loginValidator = loginValidator;
    }

    /// <summary>
    /// Autentica un usuario y devuelve un access token JWT.
    /// </summary>
    /// <param name="request">Credenciales del usuario.</param>
    /// <returns>Access token si las credenciales son válidas.</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponseDto>> Login(
        LoginDto request)
    {
        // Validamos la estructura de los datos antes
        // de intentar autenticar al usuario.
        var validationResult =
            await _loginValidator.ValidateAsync(request);

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

        var response = await _authService.LoginAsync(request);

        // Utilizamos la misma respuesta independientemente de si
        // el usuario no existe, está desactivado o la contraseña
        // es incorrecta.
        if (response is null)
        {
            return Unauthorized(new
            {
                message = "Las credenciales proporcionadas no son válidas."
            });
        }

        return Ok(response);
    }
}