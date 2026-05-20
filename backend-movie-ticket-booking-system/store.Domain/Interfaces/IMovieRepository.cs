// store.Domain/Interfaces/IMovieRepository.cs
using store.Domain.Entities;

namespace store.Domain.Interfaces;

public interface IMovieRepository
{
    Task<Movie?> GetByIdAsync(Guid id);
    Task<List<Movie>> GetAllAsync();
    Task<Movie?> CheckMovieExistAsync(string title);
    Task AddAsync(Movie movie);
    Task UpdateAsync(Movie movie);
}