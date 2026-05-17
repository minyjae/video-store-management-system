namespace Store.Application.DTOs;

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
