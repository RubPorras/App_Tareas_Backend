using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Backend.Configuration;
using Backend.Data;
using Backend.DTO.Auth;
using Backend.DTOs.Auth;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Backend.Service.AuthService
{
    /// <summary>
    /// Gestiona la autenticación de usuarios y la generación de JWT.
    /// </summary>
    public sealed class AuthService : IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(
            AppDbContext context,
            IPasswordService passwordService,
            IOptions<JwtSettings> jwtOptions)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtSettings = jwtOptions.Value;
        }

        /// <summary>
        /// Valida las credenciales de un usuario y genera un JWT
        /// cuando la autenticación es exitosa.
        /// </summary>
        public async Task<LoginResponseDto?> LoginAsync(LoginDto request)
        {
            var usernameOrEmail = request.UsernameOrEmail.Trim();

            // Buscamos por Username o Email.
            // No utilizamos PasswordHash en la consulta.
            var user = await _context.Users
                .FirstOrDefaultAsync(user =>
                    user.Username == usernameOrEmail ||
                    user.Email == usernameOrEmail.ToLower());

            // No revelamos si el usuario existe.
            if (user is null)
            {
                return null;
            }

            // Un usuario desactivado no puede iniciar sesión.
            if (!user.IsActive)
            {
                return null;
            }

            // Comprobamos la contraseña utilizando nuestro servicio.
            var passwordValid = _passwordService.VerifyPassword(
                user,
                user.PasswordHash,
                request.Password);

            if (!passwordValid)
            {
                return null;
            }

            var expiresAt = DateTime.UtcNow
                .AddMinutes(_jwtSettings.ExpirationMinutes);

            var claims = new List<Claim>
            {
                // Identificador único del usuario.
                new(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                // Rol utilizado posteriormente por autorización.
                new(
                    ClaimTypes.Role,
                    user.Role.ToString()),

                // Identificador único del token.
                new(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_jwtSettings.Secret));

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: expiresAt,
                signingCredentials: credentials);

            var accessToken = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                ExpiresAt = expiresAt
            };
        }
    }
}