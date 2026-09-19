using Backend.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Backend.Data.Configurations;

/// <summary>
/// Configuración de la entidad TaskItem para Entity Framework Core.
/// </summary>
public sealed class TaskConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        // Nombre de la tabla en SQL Server.
        builder.ToTable("Tasks");

        // Clave primaria.
        builder.HasKey(task => task.Id);

        // ID generado automáticamente por SQL Server.
        builder.Property(task => task.Id)
            .ValueGeneratedOnAdd();

        // Título obligatorio.
        builder.Property(task => task.Title)
            .IsRequired()
            .HasMaxLength(200);

        // Descripción opcional.
        builder.Property(task => task.Description)
            .HasMaxLength(2000);

        // Estado de la tarea.
        builder.Property(task => task.IsCompleted)
            .IsRequired();

        // Fecha de creación.
        builder.Property(task => task.CreatedAt)
            .IsRequired();

        // Fecha de actualización.
        builder.Property(task => task.UpdatedAt);

        // Relación User (1) -> TaskItem (N).
        builder.HasOne(task => task.User)
            .WithMany(user => user.Tasks)
            .HasForeignKey(task => task.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}