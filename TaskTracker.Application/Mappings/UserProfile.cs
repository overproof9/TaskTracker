using AutoMapper;
using TaskTracker.Domain.Entities;
using TaskTracker.Application.DTO;

namespace TaskTracker.Application.Mappings;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
    }
}