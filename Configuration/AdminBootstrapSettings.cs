namespace Backend.Configuration;

/// <summary>
/// Credenciales utilizadas únicamente para crear el primer
/// usuario administrador de la aplicación.
/// 
/// Estos valores deben proporcionarse mediante User Secrets
/// o mediante un mecanismo seguro de configuración en producción.
/// </summary>
public sealed class AdminBootstrapSettings
{
    /// <summary>
    /// Nombre de usuario del administrador inicial.
    /// </summary>
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// Correo electrónico del administrador inicial.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Contraseña inicial del administrador.
    /// Nunca debe almacenarse directamente en la base de datos.
    /// </summary>
    public string Password { get; init; } = string.Empty;
}