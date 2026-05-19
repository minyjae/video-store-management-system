// store.WebAPI/Controllers/BookingsController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Application.DTOs;
using store.Application.Interfaces;
using System.Security.Claims;

namespace store.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]                             // ต้อง Login ก่อนจองทุกครั้ง
public class BookingsController : ControllerBase
{
    private readonly ITicketBookingService _bookingService;

    public BookingsController(ITicketBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost]                          // POST /api/bookings
    public async Task<IActionResult> Book([FromBody] BookingRequestDto dto)
    {
        // ดึง UserId จาก JWT Token โดยตรง ไม่รับจาก Body (ปลอดภัยกว่า)
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not found in token.");

        var ticket = await _bookingService.BookSeatAsync(
            Guid.Parse(userId),
            dto.SeatId,
            dto.ShowtimeId);

        return Ok(new TicketDto(
            ticket.Id,
            ticket.MovieName,
            ticket.SeatCode,
            ticket.Showtime,
            ticket.PricePaid,
            ticket.ReferenceCode,
            ticket.QrCodeBase64,
            ticket.IssuedAt));
    }
}