// store.Application/DTOs/SeatDto.cs
using store.Domain.Enums;

namespace store.Application.DTOs;

public record SeatDto(
    Guid Id,
    Guid ShowtimeId,
    string SeatCode,
    SeatType Type,
    decimal Price,
    SeatStatus Status    // Frontend ใช้ Render สี ว่าง/ล็อค/จองแล้ว
);

public record CreateSeatDto(
    Guid ShowtimeId,
    string SeatCode,
    SeatType Type,
    decimal Price
);