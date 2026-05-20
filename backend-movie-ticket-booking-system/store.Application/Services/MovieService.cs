using Microsoft.EntityFrameworkCore;
using store.Application.DTOs;
using store.Application.Interfaces;
using store.Domain.Entities;
using store.Domain.Interfaces;

namespace store.Application.Services;

public class MovieService : IMovieService
{
    private readonly IMovieRepository _movieRepository;

    public MovieService(IMovieRepository movieRepository)
    {
        _movieRepository = movieRepository;
    }

    public async Task<IEnumerable<MovieDto>> GetAllAsync()
    {
        var movies = await _movieRepository.GetAllAsync();
        return movies.Select(MapToDto);
    }

    public async Task<MovieDto?> GetByIdAsync(Guid id)
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        return movie is null ? null : MapToDto(movie);
    }

    public async Task<MovieDto?> CreateAsync(CreateMovieDto dto)
    {   
        var existMovie = await _movieRepository.CheckMovieExistAsync(dto.Title);
        if (existMovie is not null)
        {
            throw new ArgumentException($"Movie title '{dto.Title}' is already in use.");
        }
        var movie = Movie.Create(dto.Title, dto.Plot, dto.Price, dto.Duration, dto.Category);

        try
        {
            await _movieRepository.AddAsync(movie);
        }
        catch (DbUpdateException)
        {
            // Safety net: race condition ผ่านมาได้ → DB unique constraint หยุด
            throw new ArgumentException($"Movie title '{dto.Title}' is already in use.");
        }

        return MapToDto(movie);
    }

    public async Task<MovieDto?> UpdateAsync(UpdateMovieDto dto)
    {      
        var existMovie = await _movieRepository.GetByIdAsync(dto.Id);

        // เช็คว่าถ้าหาไม่เจอ ให้คืนค่า null (หรือจะ throw NotFoundException ก็ได้)
        if (existMovie is null)
        {
            return null; 
        }

        existMovie.Update(dto.Title, dto.Plot, dto.Price, dto.Duration, dto.Category);

        await _movieRepository.UpdateAsync(existMovie);

        return MapToDto(existMovie);
    }

    private static MovieDto MapToDto(Movie m) => new(m.Id, m.Title, m.Plot, m.Price, m.Duration, m.Category, m.CreatedAt, m.UpdatedAt);
}