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

    public async Task<MovieDto?> GetByIdAsync(string id)  // int → string
    {
        var movie = await _movieRepository.GetByIdAsync(id);
        return movie is null ? null : MapToDto(movie);
    }

    public async Task<MovieDto> CreateAsync(CreateMovieDto dto)
    {
        var movie = Movie.Create(dto.Name, dto.Price, dto.Stock);  // ปรับให้ตรงกับ Entity

        await _movieRepository.AddAsync(movie);  // ไม่ต้อง SaveChanges

        return MapToDto(movie);
    }

    private static MovieDto MapToDto(Movie m) => new(m.Id, m.Name, m.Price, m.Stock, m.CreatedAt, m.IsActive);
}