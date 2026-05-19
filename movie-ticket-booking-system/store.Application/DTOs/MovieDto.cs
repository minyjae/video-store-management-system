using System.ComponentModel;
using store.Domain.Entities;
using store.Domain.Enums;

namespace store.Application.DTOs;

public record MovieDto(
    Guid Id,
    string Title,
    string Plot,
    decimal Price,
    TimeSpan Duration,
    MovieCategory Category,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record CreateMovieDto(
    string Title,
    string Plot,
    decimal Price,
    TimeSpan Duration,
    MovieCategory Category
);

public record UpdateMovieDto(
    Guid Id,
    string? Title = null,
    string? Plot = null,
    decimal? Price = null,
    TimeSpan? Duration = null,
    MovieCategory? Category = null
);
