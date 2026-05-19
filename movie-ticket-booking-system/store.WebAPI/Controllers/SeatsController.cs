// store.WebAPI/Controllers/SeatsController.cs
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using store.Application.DTOs;
using store.Application.Interfaces;

namespace store.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SeatsController : ControllerBase
{
    private readonly ISeatService _seatService;
    private readonly IValidator<CreateSeatDto> _createValidator;

    public SeatsController(
        ISeatService seatService,
        IValidator<CreateSeatDto> createValidator)
    {
        _seatService = seatService;
        _createValidator = createValidator;
    }

    [HttpGet("showtime/{showtimeId}")]  // GET /api/seats/showtime/{showtimeId}
    public async Task<IActionResult> GetByShowtime(Guid showtimeId)
    {
        var seats = await _seatService.GetByShowtimeAsync(showtimeId);
        return Ok(seats);
    }

    [HttpPost]                          // POST /api/seats
    public async Task<IActionResult> Create([FromBody] CreateSeatDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var seat = await _seatService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetByShowtime),
            new { showtimeId = seat.ShowtimeId }, seat);
    }
}