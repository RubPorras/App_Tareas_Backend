using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTO.User
{
    /// <summary>
    /// DTO utilizado para devolver la información de un usuario.
    /// No contiene información sensible como la contraseña.
    /// </summary>
public sealed class GetUserDto
    {
        // Identificador único del usuario.
        public int Id { get; init; }

        // Nombre de usuario.
        public string Username { get; init; } = string.Empty;

        // Correo electrónico del usuario.
        public string Email { get; init; } = string.Empty;

        // Rol actual del usuario.
        public string Role { get; init; } = string.Empty;

        // Indica si la cuenta está activa.
        public bool IsActive { get; init; }

        // Fecha en la que se creó la cuenta.
        public DateTime CreatedAt { get; init; }

        // Fecha de la última modificación de la cuenta.
        public DateTime? UpdatedAt { get; init; }
    }
}