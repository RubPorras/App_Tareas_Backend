using Backend.DTO.Task;
using FluentValidation;

namespace Backend.Validators.Task;

/// <summary>
/// Valida los datos necesarios para crear una tarea.
/// </summary>
public sealed class CreateTaskDtoValidator
    : AbstractValidator<CreateTaskDto>
{
    public CreateTaskDtoValidator()
    {
        // El título es obligatorio.
        RuleFor(task => task.Title)
            .NotEmpty()
            .WithMessage("El título es obligatorio.")

            // Evita títulos excesivamente largos.
            .MaximumLength(200)
            .WithMessage("El título no puede superar los 200 caracteres.");

        // La descripción es opcional.
        // Si existe, limitamos su longitud.
        RuleFor(task => task.Description)
            .MaximumLength(2000)
            .WithMessage(
                "La descripción no puede superar los 2000 caracteres.");
    }
}