namespace Backend.DTOs.Auth;

/// <summary>
/// Información devuelta después de una autenticación exitosa.
/// </summary>
public sealed class LoginResponseDto
{
    // Token de acceso utilizado para autenticar futuras solicitudes.
    public string AccessToken { get; init; } = string.Empty;

    // Fecha y hora UTC en la que expira el token.
    public DateTime ExpiresAt { get; init; }
}