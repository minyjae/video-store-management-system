// store.Application/Services/ShowtimeService.cs
using store.Application.DTOs;
using store.Application.Interfaces;
using store.Domain.Entities;
using store.Domain.Interfaces;

namespace store.Application.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IMovieRepository _movieRepository;

    public ShowtimeService(
        IShowtimeRepository showtimeRepository,
        IMovieRepository movieRepository)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
    }

    public async Task<ShowtimeDto?> GetByIdAsync(Guid showtimeId)
    {
        var showtime = await _showtimeRepository.GetByIdAsync(showtimeId);
        return showtime is null ? null : MapToDto(showtime);
    }

    public async Task<List<ShowtimeDto>> GetByMovieIdAsync(Guid movieId)
    {
        var showtimes = await _showtimeRepository.GetByMovieIdAsync(movieId);
        return showtimes.Select(MapToDto).ToList();
    }

    public async Task<ShowtimeDto> CreateAsync(CreateShowtimeDto dto)
    {
        var movie = await _movieRepository.GetByIdAsync(dto.MovieId)
            ?? throw new KeyNotFoundException($"Movie {dto.MovieId} not found.");

        var showtime = Showtime.Create(
            movieId:         movie.Id,
            movieName:       movie.Title,
            screenId:        dto.ScreenId,
            screenName:      dto.ScreenId, // ปรับได้ถ้ามี Screen Entity
            startTime:       dto.StartTime,
            durationMinutes: (int)movie.Duration.TotalMinutes);

        await _showtimeRepository.AddAsync(showtime);
        return MapToDto(showtime);
    }

    private static ShowtimeDto MapToDto(Showtime s) =>
        new(s.Id, s.MovieId, s.MovieName, s.ScreenId, s.ScreenName, s.StartTime, s.EndTime);
}