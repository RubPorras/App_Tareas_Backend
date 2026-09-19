using Backend.DTO.Auth;
using FluentValidation;

namespace Backend.Validators.Auth;

/// <summary>
/// Valida los datos necesarios para iniciar sesión.
/// </summary>
public sealed class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(login => login.UsernameOrEmail)
            .NotEmpty()
            .WithMessage("El nombre de usuario o correo electrónico es obligatorio.")
            .MaximumLength(254)
            .WithMessage(
                "El nombre de usuario o correo electrónico no puede superar los 254 caracteres.");

        RuleFor(login => login.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .MaximumLength(128)
            .WithMessage(
                "La contraseña no puede superar los 128 caracteres.");
    }
}