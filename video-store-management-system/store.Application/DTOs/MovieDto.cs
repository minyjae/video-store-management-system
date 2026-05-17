namespace store.Application.DTOs;

public record MovieDto(
    string Id,
    string Name,
    decimal Price,
    int Stock,
    DateTime CreatedAt,
    bool IsActive
);

public record CreateMovieDto(
    string Name,
    decimal Price,
    int Stock
);

public record UpdateMovieDto(
    string Id,
    string? Name = null,
    decimal? Price = null
);
