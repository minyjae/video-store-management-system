// store.Infrastructure/Repositories/ShowtimeRepository.cs
using Microsoft.EntityFrameworkCore;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Infrastructure.Repositories;

public class ShowtimeRepository : IShowtimeRepository
{
    private readonly AppDbContext _context;

    public ShowtimeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Showtime?> GetByIdAsync(Guid id)
        => await _context.Showtimes.FindAsync(id);

    public async Task<List<Showtime>> GetByMovieIdAsync(Guid movieId)
        => await _context.Showtimes
            .Where(s => s.MovieId == movieId)
            .ToListAsync();

    public async Task AddAsync(Showtime showtime)
    {
        await _context.Showtimes.AddAsync(showtime);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var showtime = await _context.Showtimes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Showtime {id} not found.");
        _context.Showtimes.Remove(showtime);
        await _context.SaveChangesAsync();
    }
}