// store.Domain/Interfaces/ITicketRepository.cs
using store.Domain.Entities;

namespace store.Domain.Interfaces;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<List<Ticket>> GetByUserIdAsync(Guid userId);
    Task AddAsync(Ticket ticket);
    Task DeleteByShowtimeIdAsync(Guid showtimeId);
}