// store.Infrastructure/Repositories/UserRepository.cs
using MongoDB.Driver;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IMongoCollection<User> _collection;

    public UserRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<User>("Users");
    }

    public async Task<User?> GetByUsernameAsync(string username)
        => await _collection.Find(u => u.Username == username).FirstOrDefaultAsync();

    public async Task AddAsync(User user)
        => await _collection.InsertOneAsync(user);
}