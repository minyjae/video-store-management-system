// store.WebAPI/Controllers/ShowtimesController.cs
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using store.Application.DTOs;
using store.Application.Interfaces;

namespace store.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowtimesController : ControllerBase
{
    private readonly IShowtimeService _showtimeService;
    private readonly IValidator<CreateShowtimeDto> _createValidator;

    public ShowtimesController(
        IShowtimeService showtimeService,
        IValidator<CreateShowtimeDto> createValidator)
    {
        _showtimeService = showtimeService;
        _createValidator = createValidator;
    }

    [HttpGet("{id}")]               // GET /api/showtimes/{id}
    public async Task<IActionResult> GetById(Guid id)
    {
        var showtime = await _showtimeService.GetByIdAsync(id);
        return showtime is null ? NotFound() : Ok(showtime);
    }

    [HttpGet("movie/{movieId}")]    // GET /api/showtimes/movie/{movieId}
    public async Task<IActionResult> GetByMovie(Guid movieId)
    {
        var showtimes = await _showtimeService.GetByMovieIdAsync(movieId);
        return Ok(showtimes);
    }

    [HttpPost]                      // POST /api/showtimes
    public async Task<IActionResult> Create([FromBody] CreateShowtimeDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var showtime = await _showtimeService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = showtime.Id }, showtime);
    }
}