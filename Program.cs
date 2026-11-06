using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Backend.Configuration;
using Backend.Data;
using Backend.Service.AuthService;
using Backend.Service.UserService;
using Backend.Services;
using Backend.Validators.User;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException(
        "Falta ConnectionStrings:DefaultConnection.");

var corsOptions = builder.Configuration
    .GetSection("Cors")
    .Get<CorsOptions>() ?? new CorsOptions();

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(corsOptions.AllowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<AppDbContext>("database", tags: ["ready"]);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configuration
builder.Services.AddOptions<JwtSettings>()
    .Bind(builder.Configuration.GetSection("JwtSettings"))
    .Validate(s => !string.IsNullOrWhiteSpace(s.Secret) && s.Secret.Length >= 32,
        "JwtSettings:Secret debe tener al menos 32 caracteres.")
    .ValidateOnStart();
    
builder.Services.Configure<AdminBootstrapSettings>(
    builder.Configuration.GetSection("AdminBootstrap"));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database");

// Application services
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
builder.Services.AddScoped<AdminSeeder>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();

// Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        // Obtiene la configuración JWT desde appsettings/User Secrets.
        var jwtSettings = builder.Configuration
            .GetSection("JwtSettings")
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                "La configuración JwtSettings no está disponible.");

        // Configuración utilizada para validar los tokens recibidos.
        options.TokenValidationParameters = new TokenValidationParameters
        {
            // Comprueba que el token haya sido firmado con nuestra clave.
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),

            // Comprueba quién emitió el token.
            ValidateIssuer = true,
            ValidIssuer = jwtSettings.Issuer,

            // Comprueba para quién fue emitido el token.
            ValidateAudience = true,
            ValidAudience = jwtSettings.Audience,

            // Comprueba que el token no haya expirado.
            ValidateLifetime = true,

            // Reduce ligeramente el margen permitido para tokens expirados.
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Seed the initial administrator.
using (var scope = app.Services.CreateScope())
{
    var adminSeeder = scope.ServiceProvider
        .GetRequiredService<AdminSeeder>();

    await adminSeeder.SeedAsync();
}

// Request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapHealthChecks("/health/live", new() { Predicate = r => r.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new() { Predicate = r => r.Tags.Contains("ready") });

app.MapControllers();

app.Run();

