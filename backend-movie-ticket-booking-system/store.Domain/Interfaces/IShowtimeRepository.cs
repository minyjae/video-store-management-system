// store.Domain/Interfaces/IShowtimeRepository.cs
using store.Domain.Entities;

namespace store.Domain.Interfaces;

public interface IShowtimeRepository
{
    Task<Showtime?> GetByIdAsync(Guid id);
    Task<List<Showtime>> GetByMovieIdAsync(Guid movieId);
    Task AddAsync(Showtime showtime);
    Task DeleteAsync(Guid id);
}