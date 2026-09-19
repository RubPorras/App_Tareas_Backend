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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
builder.Services.Configure<AdminBootstrapSettings>(
    builder.Configuration.GetSection("AdminBootstrap"));

// Application services
builder.Services.AddValidatorsFromAssemblyContaining<CreateUserDtoValidator>();
builder.Services.AddScoped<AdminSeeder>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IUserService, UserService>();

// Authorization
builder.Services.AddAuthorization();

// Cross-origin requests
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

var app = builder.Build();

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

app.MapControllers();

app.Run();

