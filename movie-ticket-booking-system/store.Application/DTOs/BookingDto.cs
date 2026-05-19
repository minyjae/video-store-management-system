// store.Application/DTOs/BookingDto.cs
namespace store.Application.DTOs;

public record BookingRequestDto(
    Guid SeatId,
    Guid ShowtimeId
);