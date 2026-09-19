using System.Linq;
using System.Threading.Tasks;
using Backend.DTO.User;

namespace Backend.Model
{
    // Representa un usuario registrado en la aplicación.
    // Esta clase representa la entidad que será almacenada en SQL Server.
    public sealed class User
    {
        // Identificador único del usuario.
        // SQL Server lo generará automáticamente.
        public int Id { get; set; }

        // Nombre de usuario.
        // Debe ser único dentro de la aplicación.
        public string Username { get; set; } = string.Empty;

        // Correo electrónico del usuario.
        // Debe ser único dentro de la aplicación.
        public string Email { get; set; } = string.Empty;

        // Hash de la contraseña.
        // Nunca almacenar la contraseña original.
        public string PasswordHash { get; set; } = string.Empty;

        // Rol del usuario dentro de la aplicación.
        // El usuario no podrá establecer este valor directamente durante el registro.
        public UserRole Role { get; set; } = UserRole.User;

        // Permite desactivar una cuenta sin eliminarla físicamente.
        public bool IsActive { get; set; } = true;

        // Fecha y hora en la que se creó la cuenta.
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Fecha y hora de la última modificación.
        // Puede ser null si la cuenta todavía no ha sido modificada.
        public DateTime? UpdatedAt { get; set; }
        //Relación con las tareas del usuario. 
        //Un usuario puede tener muchas tareas.
        public ICollection<TaskItem> Tasks { get; set; } = [];
    }
}