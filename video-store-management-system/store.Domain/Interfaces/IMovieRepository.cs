using store.Domain.Entities;

namespace store.Domain.Interfaces;

public interface IMovieRepository
{
    Task<IEnumerable<Movie>> GetAllAsync();   // เปลี่ยนจาก IReadOnlyList → IEnumerable
    Task<Movie?> GetByIdAsync(string id);     // เปลี่ยน int → string เพราะ MongoDB ใช้ ObjectId
    Task AddAsync(Movie movie);
}