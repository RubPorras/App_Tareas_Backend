using Backend.Model;

namespace Backend.Services;

/// <summary>
/// Define las operaciones necesarias para proteger
/// y verificar contraseñas de usuarios.
/// </summary>
public interface IPasswordService
{
    /// <summary>
    /// Genera un hash seguro a partir de una contraseña.
    /// </summary>
    string HashPassword(User user, string password);

    /// <summary>
    /// Verifica si una contraseña coincide con el hash almacenado.
    /// </summary>
    bool VerifyPassword(User user, string passwordHash, string password);
}