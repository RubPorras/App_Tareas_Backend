using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTO.User
{
    // DTO utilizado para actualizar información modificable de un usuario.
    public sealed class UpdateUserDto
    {
        // Nuevo nombre de usuario.
        public string Username { get; init; } = string.Empty;

        // Nuevo correo electrónico.
        public string Email { get; init; } = string.Empty;
    }
}