using Backend.Configuration;
using Backend.DTO.User;
using Backend.Model;
using Backend.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Backend.Data;

/// <summary>
/// Se encarga de crear el administrador inicial de la aplicación
/// cuando todavía no existe ningún usuario con rol Admin.
/// </summary>
public sealed class AdminSeeder
{
    private readonly AppDbContext _context;
    private readonly IPasswordService _passwordService;
    private readonly AdminBootstrapSettings _settings;

    public AdminSeeder(
        AppDbContext context,
        IPasswordService passwordService,
        IOptions<AdminBootstrapSettings> options)
    {
        _context = context;
        _passwordService = passwordService;
        _settings = options.Value;
    }

    /// <summary>
    /// Crea el administrador inicial si todavía no existe ninguno.
    /// </summary>
    public async Task SeedAsync()
    {
        // Si ya existe un administrador, no hacemos absolutamente nada.
        var adminExists = await _context.Users
            .AnyAsync(user => user.Role == UserRole.Admin);

        if (adminExists)
        {
            return;
        }

        // Validamos que la configuración necesaria exista.
        if (string.IsNullOrWhiteSpace(_settings.Username) ||
            string.IsNullOrWhiteSpace(_settings.Email) ||
            string.IsNullOrWhiteSpace(_settings.Password))
        {
            throw new InvalidOperationException(
                "La configuración AdminBootstrap está incompleta.");
        }

        var username = _settings.Username.Trim();
        var email = _settings.Email.Trim().ToLowerInvariant();

        // Evitamos crear el Admin si las credenciales configuradas
        // ya pertenecen a otro usuario.
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(user =>
                user.Username == username ||
                user.Email == email);

        if (existingUser is not null)
        {
            throw new InvalidOperationException(
                "Las credenciales configuradas para AdminBootstrap " +
                "ya pertenecen a un usuario existente.");
        }

        // Creamos el administrador inicial.
        var admin = new User
        {
            Username = username,
            Email = email,
            Role = UserRole.Admin,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        // La contraseña nunca se almacena directamente.
        admin.PasswordHash = _passwordService.HashPassword(
            admin,
            _settings.Password);

        _context.Users.Add(admin);

        await _context.SaveChangesAsync();
    }
}