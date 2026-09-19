using Backend.Model;
using Microsoft.AspNetCore.Identity;

namespace Backend.Services;

/// <summary>
/// Implementa la protección y verificación de contraseñas
/// utilizando el PasswordHasher proporcionado por ASP.NET Core.
/// </summary>
public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<User> _passwordHasher;

    /// <summary>
    /// Constructor del servicio de hashing.
    /// </summary>
    public PasswordService()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    /// <summary>
    /// Genera un hash seguro para la contraseña proporcionada.
    /// </summary>
    public string HashPassword(User user, string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        return _passwordHasher.HashPassword(user, password);
    }

    /// <summary>
    /// Comprueba si la contraseña proporcionada coincide
    /// con el hash almacenado.
    /// </summary>
    public bool VerifyPassword(
        User user,
        string passwordHash,
        string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            passwordHash,
            password);

        return result is PasswordVerificationResult.Success
            or PasswordVerificationResult.SuccessRehashNeeded;
    }
}