// store.Infrastructure/Repositories/MovieRepository.cs
using Microsoft.EntityFrameworkCore;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Infrastructure.Repositories;

public class MovieRepository : IMovieRepository
{
    private readonly AppDbContext _context;

    public MovieRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Movie?> GetByIdAsync(Guid id)
        => await _context.Movies.FindAsync(id);

    public async Task<List<Movie>> GetAllAsync()
        => await _context.Movies.ToListAsync();

    public async Task AddAsync(Movie movie)
    {
        await _context.Movies.AddAsync(movie);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Movie movie)
    {
        _context.Movies.Update(movie);
        await _context.SaveChangesAsync();
    }

    public async Task<Movie?> CheckMovieExistAsync(string title)
        => await _context.Movies.FirstOrDefaultAsync(m => m.Title == title.Trim());
}