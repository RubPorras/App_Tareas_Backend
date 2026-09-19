using Microsoft.EntityFrameworkCore;
using Backend.Model;

namespace Backend.Data
{
    public sealed class AppDbContext : DbContext
    {
        // Constructor utilizado por el sistema de inyección
        // de dependencias de ASP.NET Core.
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
            
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        // Representa la tabla Users dentro de SQL Server.
        public DbSet<User> Users => Set<User>();
        public DbSet<TaskItem> Tasks => Set<TaskItem>();
    }
}