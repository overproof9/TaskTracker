using AutoMapper;
using TaskTracker.Application.DTO;
using TaskTracker.Application.Interfaces;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Application.Services;

internal class UserService(
    IGenericRepository<User> userRepository,
    IMapper mapper
) : IUserService
{
    private readonly IGenericRepository<User> _userRepository = userRepository;
    private readonly IMapper _mapper = mapper;

    public async Task<UserDto> CreateAsync(string name, string email)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Name = name,
            Email = email
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return _mapper.Map<UserDto>(user);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null) return false;

        _userRepository.Delete(user);
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateAsync(Guid id, string name)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null) return false;

        user.Name = name;

        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _userRepository.GetByIdAsync(id);
        return user is null ? null : _mapper.Map<UserDto>(user);
    }

    public async Task<IEnumerable<UserDto>> GetAllAsync()
    {
        var users = await _userRepository.GetAllAsync();
        return _mapper.Map<IEnumerable<UserDto>>(users);
    }
}
