using Backend.DTO.Task;
using FluentValidation;

namespace Backend.Validators.Task;

/// <summary>
/// Valida los datos utilizados para actualizar una tarea.
/// </summary>
public sealed class UpdateTaskDtoValidator
    : AbstractValidator<UpdateTaskDto>
{
    public UpdateTaskDtoValidator()
    {
        // El título sigue siendo obligatorio durante una actualización.
        RuleFor(task => task.Title)
            .NotEmpty()
            .WithMessage("El título es obligatorio.")

            .MaximumLength(200)
            .WithMessage("El título no puede superar los 200 caracteres.");

        // La descripción es opcional.
        RuleFor(task => task.Description)
            .MaximumLength(2000)
            .WithMessage(
                "La descripción no puede superar los 2000 caracteres.");

        // IsCompleted es un bool, por lo que no requiere
        // una validación adicional.
    }
}