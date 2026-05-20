// store.Domain/Interfaces/IUserRepository.cs
using store.Domain.Entities;

namespace store.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(Guid id);
    Task AddAsync(User user);
}