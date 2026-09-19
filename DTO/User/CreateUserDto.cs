using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTO.User
{
    // DTO utilizado para registrar un nuevo usuario.
    public sealed class CreateUserDto
    {
        // Nombre de usuario elegido por el usuario.
        public string Username { get; init; } = string.Empty;

        // Correo electrónico del usuario.
        public string Email { get; init; } = string.Empty;

        // Contraseña utilizada para crear la cuenta.
        public string Password { get; init; } = string.Empty;
    }
}