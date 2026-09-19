using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Model
{
    public sealed class TaskItem
    {
        public int Id { get; set; }

        // Título breve de la tarea.
        public string Title { get; set; } = string.Empty;

        // Descripción opcional de la tarea.
        public string? Description { get; set; }

        // Indica si la tarea ya fue completada.
        public bool IsCompleted { get; set; }

        // Usuario propietario de la tarea.
        public int UserId { get; set; }

        // Navegación hacia el usuario propietario.
        public User User { get; set; } = null!;

        // Fecha de creación.
        public DateTime CreatedAt { get; set; }

        // Fecha de última modificación.
        public DateTime? UpdatedAt { get; set; }
    }
}