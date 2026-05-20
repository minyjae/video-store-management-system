// store.Application/Interfaces/ISeatService.cs
using store.Application.DTOs;

namespace store.Application.Interfaces;

public interface ISeatService
{
    Task<List<SeatDto>> GetByShowtimeAsync(Guid showtimeId); // ดู Layout ที่นั่งทั้งหมด
    Task<SeatDto> CreateAsync(CreateSeatDto dto);
}