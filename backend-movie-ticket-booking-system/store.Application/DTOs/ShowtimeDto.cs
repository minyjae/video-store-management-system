// store.Application/DTOs/ShowtimeDto.cs
namespace store.Application.DTOs;

public record ShowtimeDto(
    Guid Id,
    Guid MovieId,
    string MovieName,
    string ScreenId,
    string ScreenName,
    DateTime StartTime,
    DateTime EndTime
);

public record CreateShowtimeDto(
    Guid MovieId,
    string ScreenId,
    DateTime StartTime
);

public record DeleteShowtimeDto(
    Guid Id
);