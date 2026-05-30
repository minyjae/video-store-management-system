namespace store.Domain.Entities;

public class Banner
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string ImageUrl { get; private set; } = string.Empty;
    public string Title { get; private set; } = string.Empty;
    public string Tagline { get; private set; } = string.Empty;
    public string Genre { get; private set; } = string.Empty;
    public int DisplayOrder { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Banner() {}

    public static Banner Create(string imageUrl, string title, string tagline, string genre, int displayOrder) => new()
    {
        Id = Guid.NewGuid(),
        ImageUrl = imageUrl,
        Title = title.Trim(),
        Tagline = tagline.Trim(),
        Genre = genre.Trim(),
        DisplayOrder = displayOrder,
        CreatedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok"),
    };

    public void Update(string? title, string? tagline, string? genre)
    {
        if (!string.IsNullOrWhiteSpace(title)) Title = title.Trim();
        if (!string.IsNullOrWhiteSpace(tagline)) Tagline = tagline.Trim();
        if (!string.IsNullOrWhiteSpace(genre)) Genre = genre.Trim();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok");
    }
}
