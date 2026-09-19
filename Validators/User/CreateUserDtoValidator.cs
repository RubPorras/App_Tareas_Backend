using Backend.DTO.User;
using FluentValidation;

namespace Backend.Validators.User;

/// <summary>
/// Valida los datos necesarios para crear un usuario.
/// </summary>
public sealed class CreateUserDtoValidator : AbstractValidator<CreateUserDto>
{
    public CreateUserDtoValidator()
    {
        RuleFor(user => user.Username)
            .NotEmpty()
            .WithMessage("El nombre de usuario es obligatorio.")
            .MinimumLength(3)
            .WithMessage("El nombre de usuario debe tener al menos 3 caracteres.")
            .MaximumLength(50)
            .WithMessage("El nombre de usuario no puede superar los 50 caracteres.")
            .Matches(@"^[a-zA-Z0-9_.-]+$")
            .WithMessage(
                "El nombre de usuario solo puede contener letras, números, '.', '_' y '-'.");
        
        RuleFor(user => user.Email)
            .NotEmpty()
            .WithMessage("El correo electrónico es obligatorio.")
            .EmailAddress()
            .WithMessage("El correo electrónico no tiene un formato válido.")
            .MaximumLength(254)
            .WithMessage("El correo electrónico no puede superar los 254 caracteres.");

        RuleFor(user => user.Password)
            .NotEmpty()
            .WithMessage("La contraseña es obligatoria.")
            .MinimumLength(12)
            .WithMessage("La contraseña debe tener al menos 12 caracteres.")
            .MaximumLength(128)
            .WithMessage("La contraseña no puede superar los 128 caracteres.");
    }
}