using TaskTracker.Application.DTO;

namespace TaskTracker.Application.Interfaces
{
    public interface IUserService
    {
        Task<UserDto> CreateAsync(string name, string email);
        Task<bool> DeleteAsync(Guid id);
        Task<bool> UpdateAsync(Guid id, string name);
        Task<UserDto?> GetByIdAsync(Guid id);
        Task<IEnumerable<UserDto>> GetAllAsync();
    }
}