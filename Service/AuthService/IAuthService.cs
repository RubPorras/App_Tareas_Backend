using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.DTO.Auth;
using Backend.DTOs.Auth;

namespace Backend.Service.AuthService
{
    /// <summary>
    /// Define las operaciones relacionadas con autenticación.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Valida las credenciales y genera un access token.
        /// </summary>
        Task<LoginResponseDto?> LoginAsync(LoginDto request);
    }
}