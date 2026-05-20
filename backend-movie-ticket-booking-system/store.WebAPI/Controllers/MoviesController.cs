using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Application.DTOs;
using store.Application.Interfaces;

namespace store.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MoviesController : ControllerBase
{
    private readonly IMovieService _movieService;
    private readonly IValidator<CreateMovieDto> _createValidator;
    private readonly IValidator<UpdateMovieDto> _updateValidator;

    public MoviesController(
        IMovieService movieService,
        IValidator<CreateMovieDto> createValidator,
        IValidator<UpdateMovieDto> updateValidator)
    {
        _movieService = movieService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]                // ← เพิ่ม
    public async Task<IActionResult> GetAll()
    {
        var movies = await _movieService.GetAllAsync();
        return Ok(movies);
    }

    [HttpGet("{id}")]        // ← เพิ่ม
    public async Task<IActionResult> GetById(Guid id)
    {
        var movie = await _movieService.GetByIdAsync(id);
        return movie is null ? NotFound() : Ok(movie);
    }

    [HttpPost]               // ← เพิ่ม
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateMovieDto dto)
    {
        var validation = await _createValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var movie = await _movieService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = movie.Id }, movie);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateMovieDto dto)
    {
        var validation = await _updateValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));
        
        var movie = await _movieService.UpdateAsync(dto);
        return movie is null ? NotFound() : Ok(movie);
    }
}