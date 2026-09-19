using System.Linq;
using System.Threading.Tasks;

namespace Backend.DTO.User
{
    // DTO reducido utilizado cuando solamente necesitamos información básica del usuario.
    public sealed class UserSummaryDto
    {
        /// <summary>
        /// Identificador único del usuario.
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// Nombre de usuario.
        /// </summary>
        public string Username { get; init; } = string.Empty;
    }
}