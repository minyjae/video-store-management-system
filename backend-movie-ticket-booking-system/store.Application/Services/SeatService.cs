// store.Application/Services/SeatService.cs
using store.Application.DTOs;
using store.Application.Interfaces;
using store.Domain.Entities;
using store.Domain.Enums;
using store.Domain.Interfaces;

namespace store.Application.Services;

public class SeatService : ISeatService
{
    private readonly ISeatRepository _seatRepository;

    public SeatService(ISeatRepository seatRepository)
    {
        _seatRepository = seatRepository;
    }

    public async Task<List<SeatDto>> GetByShowtimeAsync(Guid showtimeId)
    {
        var seats = await _seatRepository.GetByShowtimeAsync(showtimeId);
        return seats.Select(MapToDto).ToList();
    }

    public async Task<SeatDto> CreateAsync(CreateSeatDto dto)
    {
        var seat = Seat.Create(
            showtimeId: dto.ShowtimeId,
            seatCode:   dto.SeatCode,
            type:       dto.Type,
            price:      dto.Price);

        await _seatRepository.AddAsync(seat);
        return MapToDto(seat);
    }

    private static SeatDto MapToDto(Seat s) =>
        new(s.Id, s.ShowtimeId, s.SeatCode, s.Type, s.Price, s.Status);
}