// store.Application/DTOs/TicketDto.cs
namespace store.Application.DTOs;

public record TicketDto(
    Guid Id,
    string MovieName,
    string SeatCode,
    DateTime Showtime,
    decimal PricePaid,
    string ReferenceCode,
    string QrCodeBase64,
    DateTime IssuedAt
);