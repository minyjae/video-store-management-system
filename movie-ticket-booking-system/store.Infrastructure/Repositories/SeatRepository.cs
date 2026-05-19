// store.Infrastructure/Repositories/SeatRepository.cs
using Microsoft.EntityFrameworkCore;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Infrastructure.Repositories;

public class SeatRepository : ISeatRepository
{
    private readonly AppDbContext _context;

    public SeatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Seat?> GetByIdAsync(Guid id)
        => await _context.Seats.FindAsync(id);

    public async Task<List<Seat>> GetByShowtimeAsync(Guid showtimeId)
        => await _context.Seats
            .Where(s => s.ShowtimeId == showtimeId)
            .ToListAsync();

    public async Task AddAsync(Seat seat)
    {
        await _context.Seats.AddAsync(seat);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Seat seat)
    {
        // EF Core Track การเปลี่ยนแปลงอัตโนมัติ
        // xmin OCC จะ throw DbUpdateConcurrencyException ถ้ามีคนแก้ก่อน
        await _context.SaveChangesAsync();
    }
}