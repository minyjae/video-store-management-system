// store.Infrastructure/Repositories/TicketRepository.cs
using Microsoft.EntityFrameworkCore;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Infrastructure.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
        => await _context.Tickets.FindAsync(id);

    public async Task<List<Ticket>> GetByUserIdAsync(Guid userId)
        => await _context.Tickets
            .Where(t => t.UserId == userId)
            .ToListAsync();

    public async Task AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        // ไม่ SaveChanges ที่นี่ — ให้ Transaction ใน Service จัดการ
    }

    public async Task DeleteByShowtimeIdAsync(Guid showtimeId)
    {
        await _context.Tickets
            .Where(t => t.ShowtimeId == showtimeId)
            .ExecuteDeleteAsync();
    }
}