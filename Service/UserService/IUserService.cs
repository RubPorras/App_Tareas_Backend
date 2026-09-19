using Backend.DTO.User;

namespace Backend.Service.UserService
{
    // Define las operaciones disponibles para la gestión de usuarios.
    public interface IUserService
    {
        // Obtiene todos los usuarios.
        Task<IEnumerable<GetUserDto>> GetAllAsync();
        // Obtiene un usuario mediante su identificador.
        Task<GetUserDto?> GetByIdAsync(int id);
        // Crea un nuevo usuario.
        Task<GetUserDto> CreateAsync(CreateUserDto request);
        // Actualiza la información de un usuario
        Task<GetUserDto?> UpdateAsync(int id, UpdateUserDto request);
        // Elimina un usuario
        Task<bool> DeleteAsync(int id);
        // Promueve un usuario al rol de administrador.
        Task<bool> PromoteAsync(int id);
        // Degrada un administrador al rol de usuario.
        Task<bool> DemoteAsync(int id);
        // Activa o desactiva un usuario.
        Task<bool> ToggleActiveAsync(int id, int requestingUserId);
    }
}