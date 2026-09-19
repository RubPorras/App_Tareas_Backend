namespace Backend.DTO.Auth;

/// <summary>
/// Datos necesarios para iniciar sesión.
/// </summary>
public sealed class LoginDto
{
    // Puede ser el Username o el Email del usuario.
    public string UsernameOrEmail { get; init; } = string.Empty;

    // Contraseña proporcionada durante el inicio de sesión.
    public string Password { get; init; } = string.Empty;
}