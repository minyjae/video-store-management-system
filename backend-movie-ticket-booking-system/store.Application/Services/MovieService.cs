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

    public async Task<MovieDto> CreateAsync(CreateMovieDto dto, IFormFile poster, string webRootPath)
    {
        var existMovie = await _movieRepository.CheckMovieExistAsync(dto.Title);
        if (existMovie is not null)
            throw new ArgumentException($"Movie title '{dto.Title}' is already in use.");

        // 1. Save poster first — if this fails, movie is never created
        var ext = Path.GetExtension(poster.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var uploadPath = Path.Combine(webRootPath, "uploads", "posters");
        Directory.CreateDirectory(uploadPath);
        var filePath = Path.Combine(uploadPath, fileName);

        await using (var stream = File.Create(filePath))
            await poster.CopyToAsync(stream);

        // 2. Create movie with posterUrl already set
        var movie = Movie.Create(dto.Title, dto.Plot, dto.Price, dto.Duration, dto.Category);
        movie.SetPosterUrl($"/uploads/posters/{fileName}");

        try
        {
            await _movieRepository.AddAsync(movie);
        }
        catch (DbUpdateException)
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            throw new ArgumentException($"Movie title '{dto.Title}' is already in use.");
        }
        catch
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            throw;
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

    public async Task<MovieDto> UploadPosterAsync(Guid movieId, IFormFile file, string webRootPath)
    {
        var movie = await _movieRepository.GetByIdAsync(movieId)
            ?? throw new KeyNotFoundException($"Movie {movieId} not found.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var uploadPath = Path.Combine(webRootPath, "uploads", "posters");
        Directory.CreateDirectory(uploadPath);

        var filePath = Path.Combine(uploadPath, fileName);
        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);

        movie.SetPosterUrl($"/uploads/posters/{fileName}");
        await _movieRepository.UpdateAsync(movie);

        return MapToDto(movie);
    }

    private static MovieDto MapToDto(Movie m) =>
        new(m.Id, m.Title, m.Plot, m.Price, m.Duration, m.Category, m.PosterUrl ?? string.Empty, m.CreatedAt, m.UpdatedAt);
}