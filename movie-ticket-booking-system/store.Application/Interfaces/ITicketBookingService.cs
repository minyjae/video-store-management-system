// store.Application/Interfaces/ITicketBookingService.cs
using store.Domain.Entities;

namespace store.Application.Interfaces;

public interface ITicketBookingService
{
    Task<Ticket> BookSeatAsync(Guid userId, Guid seatId, Guid showtimeId);
}