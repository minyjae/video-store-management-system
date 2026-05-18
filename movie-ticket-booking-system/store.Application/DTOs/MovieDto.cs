using System.ComponentModel;
using store.Domain.Entities;
using store.Domain.Enums;

namespace store.Application.DTOs;

public record MovieDto(
    string Id,
    string Title,
    string Plot,
    decimal Price,
    TimeSpan Duration,
    MovieCategory Category,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    bool IsActive
);

public record CreateMovieDto(
    string Title,
    string Plot,
    decimal Price,
    TimeSpan Duration,
    MovieCategory Category
);

public record UpdateMovieDto(
    string Id,
    string? Title = null,
    string? Plot = null,
    decimal? Price = null,
    TimeSpan? Duration = null,
    MovieCategory? Category = null
);
