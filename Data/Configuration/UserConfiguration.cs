using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Model;

namespace Backend.Data.Configuration
{
// Configuración de Entity Framework Core para la entidad User
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> entity)
    {
        // Clave primaria.
        entity.HasKey(user => user.Id);

        // Username obligatorio, con un máximo de 50 caracteres.
        entity.Property(user => user.Username)
            .IsRequired()
            .HasMaxLength(50);

        // Impide que existan dos usuarios con el mismo Username.
        entity.HasIndex(user => user.Username)
            .IsUnique();

        // Email obligatorio.
        entity.Property(user => user.Email)
            .IsRequired()
            .HasMaxLength(254);

        // Impide que existan dos usuarios con el mismo Email.
        entity.HasIndex(user => user.Email)
            .IsUnique();

        // Almacenamos el hash, nunca la contraseña original.
        entity.Property(user => user.PasswordHash)
            .IsRequired()
            .HasMaxLength(255);

        // El enum UserRole se almacena como entero.
        entity.Property(user => user.Role)
            .HasConversion<int>()
            .IsRequired();

        // La cuenta comienza activa.
        entity.Property(user => user.IsActive)
            .IsRequired();

        // Fecha de creación obligatoria.
        entity.Property(user => user.CreatedAt)
            .IsRequired();

        // Puede ser NULL si todavía no ha sido modificada.
        entity.Property(user => user.UpdatedAt)
            .IsRequired(false);
        }
    }
}