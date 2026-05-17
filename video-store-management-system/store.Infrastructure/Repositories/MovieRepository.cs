using MongoDB.Driver;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly IMongoCollection<Movie> _collection;

    public MovieRepository(MongoDbContext context)
    {
        _collection = context.GetCollection<Movie>("Movies");
    }

    public async Task<IEnumerable<Movie>> GetAllAsync()
        => await _collection.Find(_ => true).ToListAsync();

    public async Task<Movie?> GetByIdAsync(string id)
        => await _collection.Find(m => m.Id == id).FirstOrDefaultAsync();

    public async Task AddAsync(Movie movie)
        => await _collection.InsertOneAsync(movie);

    public async Task UpdateAsync(Movie movie)
        => await _collection.ReplaceOneAsync(m => m.Id == movie.Id, movie);
}