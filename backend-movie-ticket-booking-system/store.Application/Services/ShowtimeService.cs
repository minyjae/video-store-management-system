// store.Application/Services/ShowtimeService.cs
using store.Application.DTOs;
using store.Application.Interfaces;
using store.Domain.Entities;
using store.Domain.Enums;
using store.Domain.Interfaces;

namespace store.Application.Services;

public class ShowtimeService : IShowtimeService
{
    private readonly IShowtimeRepository _showtimeRepository;
    private readonly IMovieRepository _movieRepository;
    private readonly ISeatRepository _seatRepository;
    private readonly ITicketRepository _ticketRepository;

    // Layout matches the frontend cinema UI
    private static readonly (string Row, int Cols, SeatType Type)[] SeatLayout =
    [
        ("A", 8,  SeatType.VIP),
        ("B", 8,  SeatType.VIP),
        ("C", 12, SeatType.Normal),
        ("D", 12, SeatType.Normal),
        ("E", 12, SeatType.Normal),
        ("F", 12, SeatType.Normal),
        ("G", 12, SeatType.Normal),
        ("H", 12, SeatType.Normal),
    ];

    public ShowtimeService(
        IShowtimeRepository showtimeRepository,
        IMovieRepository movieRepository,
        ISeatRepository seatRepository,
        ITicketRepository ticketRepository)
    {
        _showtimeRepository = showtimeRepository;
        _movieRepository = movieRepository;
        _seatRepository = seatRepository;
        _ticketRepository = ticketRepository;
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
        await GenerateSeatsAsync(showtime.Id, movie.Price);
        return MapToDto(showtime);
    }

    private async Task GenerateSeatsAsync(Guid showtimeId, decimal moviePrice)
    {
        var normalPrice = moviePrice;
        var vipPrice    = Math.Round(moviePrice * 2.5m, 2);

        var seats = new List<Seat>();
        foreach (var (row, cols, type) in SeatLayout)
        {
            var price = type == SeatType.VIP ? vipPrice : normalPrice;
            for (int col = 1; col <= cols; col++)
                seats.Add(Seat.Create(showtimeId, $"{row}{col}", type, price));
        }

        await _seatRepository.AddRangeAsync(seats);
    }

    public async Task DeleteAsync(DeleteShowtimeDto dto)
    {
        await _ticketRepository.DeleteByShowtimeIdAsync(dto.Id);
        await _seatRepository.DeleteByShowtimeIdAsync(dto.Id);
        await _showtimeRepository.DeleteAsync(dto.Id);
    }

    private static ShowtimeDto MapToDto(Showtime s) =>
        new(s.Id, s.MovieId, s.MovieName, s.ScreenId, s.ScreenName, s.StartTime, s.EndTime, s.IsActive);
}