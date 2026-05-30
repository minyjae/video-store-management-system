namespace store.Application.DTOs;

public record BannerDto(Guid Id, string ImageUrl, string Title, string Tagline, string Genre, int DisplayOrder, DateTime CreatedAt);

public record CreateBannerDto(string Title, string Tagline, string Genre);

public record UpdateBannerDto(Guid Id, string? Title, string? Tagline, string? Genre);
