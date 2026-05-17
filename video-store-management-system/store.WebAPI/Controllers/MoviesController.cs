using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Store.Application.DTOs;
using Store.Application.Interfaces;

namespace Store.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly IValidator<CreateMovieDto> _createValidator;

    public MoviesController(
        IMovieService movieService,
        IValidator<CreateMovieDto> createValidator)
    {
        _movieService = movieService;
        _createValidator = createValidator;
    }

    [HttpGet]                // ← เพิ่ม
    public async Task<IActionResult> GetAll()
    {
        var movies = await _movieService.GetAllAsync();
        return Ok(movies);
    }

    [HttpGet("{id}")]        // ← เพิ่ม
    public async Task<IActionResult> GetById(string id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        return movie is null ? NotFound() : Ok(movie);
    }

    [HttpPost]               // ← เพิ่ม
    public async Task<IActionResult> Create([FromBody] CreateMovieDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var movie = await _movieService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }
}