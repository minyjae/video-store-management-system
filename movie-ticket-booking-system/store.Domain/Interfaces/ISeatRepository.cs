// store.Domain/Interfaces/ISeatRepository.cs
using store.Domain.Entities;

namespace store.Domain.Interfaces;

public interface ISeatRepository
{
    Task<Seat?> GetByIdAsync(Guid id);
    Task<List<Seat>> GetByShowtimeAsync(Guid showtimeId);
    Task AddAsync(Seat seat);
    Task UpdateAsync(Seat seat); // EF Core Track การเปลี่ยนแปลงเอง
}