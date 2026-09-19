using Backend.Data;
using Backend.DTO.User;
using Backend.Model;
using Backend.Services;
using Microsoft.EntityFrameworkCore;

namespace Backend.Service.UserService
{
    // Implementa las operaciones relacionadas con los usuarios.
    public sealed class UserService : IUserService
    {
        private readonly AppDbContext _context;
        private readonly IPasswordService _passwordService;
        
        /* Constructor del servicio.
        AppDbContext es proporcionado mediante inyección de dependencias. */
        public UserService(AppDbContext context, IPasswordService passwordService)
        {
            _context = context;
            _passwordService = passwordService;
        }
        // Obtiene todos los usuarios.
        public async Task<IEnumerable<GetUserDto>> GetAllAsync()
        {
        // Solo seleccionamos los campos necesarios para el DTO.
        // De esta manera nunca exponemos PasswordHash.
            return await _context.Users
            .AsNoTracking()
            .Select(user => new GetUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            })
                .ToListAsync();
            }
        // Obtiene un usuario mediante su identificador.
        public async Task<GetUserDto?> GetByIdAsync(int id)
        {
        // Buscamos únicamente los datos que necesita la respuesta.
            return await _context.Users
            .AsNoTracking()
            .Where(user => user.Id == id)
            .Select(user => new GetUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            })
                .FirstOrDefaultAsync();
            }
        /* Crea un nuevo usuario.
        La implementación se completará cuando incorporemos
        el servicio seguro de hashing de contraseñas. */
        public async Task<GetUserDto> CreateAsync(CreateUserDto request)
        {
            // Normalizamos los datos antes de realizar cualquier operación
            // para evitar registros inconsistentes.
            var username = request.Username.Trim();
            var email = request.Email.Trim().ToLowerInvariant();

            // Comprobamos si ya existe un usuario con el mismo Username
            // o Email. La comprobación se realiza antes de intentar insertar.
            var exists = await _context.Users
                .AnyAsync(user =>
                    user.Username == username ||
                    user.Email == email);

            if (exists)
            {
                throw new InvalidOperationException(
                    "El nombre de usuario o correo electrónico ya está registrado.");
            }

            // Creamos la entidad. El Role NO procede del cliente.
            // Un usuario registrado públicamente siempre comienza como User.
            var user = new User
            {
                Username = username,
                Email = email,
                Role = UserRole.User,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // La contraseña nunca se almacena directamente.
            // PasswordService genera el hash seguro.
            user.PasswordHash = _passwordService.HashPassword(
                user,
                request.Password);

            // Guardamos el usuario en SQL Server.
            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            // Devolvemos únicamente la información pública del usuario.
            return new GetUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
        
        /// <summary>
        /// Actualiza la información modificable de un usuario.
        /// </summary>
        public async Task<GetUserDto?> UpdateAsync(
            int id,
            UpdateUserDto request)
        {
            // Buscamos el usuario que se desea actualizar.
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == id);

            // Si no existe, el controlador podrá responder con HTTP 404.
            if (user is null)
            {
                return null;
            }

            // Normalizamos los datos antes de comprobar duplicados y actualizar.
            var username = request.Username.Trim();
            var email = request.Email.Trim().ToLowerInvariant();

            // Comprobamos si otro usuario ya utiliza el Username o Email.
            // Excluimos al usuario actual mediante user.Id != id.
            var duplicateExists = await _context.Users
                .AnyAsync(existingUser =>
                    existingUser.Id != id &&
                    (existingUser.Username == username ||
                    existingUser.Email == email));

            if (duplicateExists)
            {
                throw new InvalidOperationException(
                    "El nombre de usuario o correo electrónico ya está registrado.");
            }

            // Solo modificamos las propiedades permitidas por UpdateUserDto.
            user.Username = username;
            user.Email = email;

            // UpdatedAt representa el momento de la última modificación.
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Devolvemos únicamente información pública.
            return new GetUserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role.ToString(),
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }

        // Elimina un usuario
        public async Task<bool> DeleteAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == id);

        // No existe ningún usuario con ese ID.
            if (user is null)
            {
                return false;
            }

            if (user.Role == UserRole.Admin)
            {
                var adminCount = await _context.Users
                    .CountAsync(u => u.Role == UserRole.Admin);

                if (adminCount <= 1)
                {
                    throw new InvalidOperationException(
                        "No se puede eliminar al último administrador.");
                }
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return true;
        }

        // Promueve un usuario existente al rol Admin.
        public async Task<bool> PromoteAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == id);

            if (user is null)
            {
                return false;
            }

            // Solo User puede ser promovido mediante esta operación.
            if (user.Role != UserRole.User)
            {
                return false;
            }

            user.Role = UserRole.Admin;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // Degrada un administrador a User.
        public async Task<bool> DemoteAsync(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == id);

            if (user is null)
            {
                return false;
            }

            // Solo Admin puede ser degradado mediante esta operación.
            if (user.Role != UserRole.Admin)
            {
                return false;
            }

            // Nunca debemos permitir que el sistema se quede
            // sin ningún administrador.
            var adminCount = await _context.Users
                .CountAsync(user => user.Role == UserRole.Admin);

            if (adminCount <= 1)
            {
                throw new InvalidOperationException(
                    "No se puede degradar al último administrador.");
            }

            user.Role = UserRole.User;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }

        // Activa o desactiva un usuario.
        public async Task<bool> ToggleActiveAsync(int id, int requestingUserId)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(user => user.Id == id);

            // No existe ningún usuario con ese ID.
            if (user is null)
            {
                return false;
            }

            // Un administrador no puede desactivarse a sí mismo.
            if (user.Id == requestingUserId && user.Role == UserRole.Admin)
            {
                throw new InvalidOperationException(
                    "Un administrador no puede desactivarse a sí mismo.");
            }

            // Invertimos el estado actual.
            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}