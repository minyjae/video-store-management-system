// store.Infrastructure/Repositories/UserRepository.cs
using Microsoft.EntityFrameworkCore;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByUsernameAsync(string username)
        => await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

    public async Task<User?> GetByIdAsync(Guid id)
        => await _context.Users.FindAsync(id);

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
    }
}