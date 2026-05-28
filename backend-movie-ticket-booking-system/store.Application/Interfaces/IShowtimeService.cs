// store.Application/Interfaces/IShowtimeService.cs
using store.Application.DTOs;

namespace store.Application.Interfaces;

public interface IShowtimeService
{
    Task<ShowtimeDto?> GetByIdAsync(Guid showtimeId);
    Task<List<ShowtimeDto>> GetByMovieIdAsync(Guid movieId);
    Task<ShowtimeDto> CreateAsync(CreateShowtimeDto dto);
    Task DeleteAsync(DeleteShowtimeDto dto);
}