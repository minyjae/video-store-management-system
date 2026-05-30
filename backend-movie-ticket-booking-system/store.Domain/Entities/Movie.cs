// store.Domain/Entities/Movie.cs
using store.Domain.Enums;
namespace store.Domain.Entities;

public class Movie
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Title { get; private set; } = string.Empty;
    public string Plot { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public TimeSpan Duration { get; private set; }
    public MovieCategory Category { get; private set; }
    public string? PosterUrl { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Movie() {}

    public static Movie Create(string title, string plot, decimal price,
                               TimeSpan duration, MovieCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Movie name is required.");
        if (duration.TotalMinutes <= 0)
            throw new ArgumentException("Duration must be positive.");

        return new Movie
        {
            Title      = title.Trim(),
            Plot  = plot,
            Price = price,
            Duration  = duration,
            Category  = category,
            CreatedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok"),
            UpdatedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok")
        };
    }    
    public void Update(string? newTitle, string? newPlot, decimal? newPrice, TimeSpan? newDuration, MovieCategory? newCategory)
    {
        // ถ้าส่งชื่อใหม่มา (ไม่เป็น null หรือค่าว่าง) ให้แทนที่ชื่อเดิม
        if (!string.IsNullOrWhiteSpace(newTitle))
        {
            Title = newTitle.Trim();
        }

        if (!string.IsNullOrWhiteSpace(newPlot))
        {
            Plot = newPlot;
        }

        if (newDuration.HasValue && newDuration.Value >= TimeSpan.Zero)
        {
            Duration = newDuration.Value;
        }

        // ถ้าส่งราคาใหม่มา และราคามากกว่า 0 ให้แทนที่ราคาเดิม
        if (newPrice.HasValue && newPrice.Value > 0)
        {
            Price = newPrice.Value;
        }

        if (newCategory.HasValue)
        {
            Category = newCategory.Value;
        }

        UpdatedAt =  TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok");
    }

    public void SetPosterUrl(string url)
    {
        PosterUrl = url;
        UpdatedAt = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok");
    }
}

