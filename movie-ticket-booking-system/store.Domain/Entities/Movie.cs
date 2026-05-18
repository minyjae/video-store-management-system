using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using store.Domain.Enums;

namespace store.Domain.Entities;

public class Movie
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; } = ObjectId.GenerateNewId().ToString();
    public string Title { get; private set; } = string.Empty;
    public string Plot { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public TimeSpan Duration { get; private set; }
    public MovieCategory Category { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Movie() {}

    public static Movie Create(string title, string plot, decimal price, TimeSpan duration, MovieCategory category)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Pls named that movie");
        }

        if (price <= 0)
        {
            throw new ArgumentException("That price must more than 0");
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentException("Duration of movie must more than 0.");
        }

        return new Movie
        {
            Title = title.Trim(),
            Plot = plot,
            Price = price,
            Duration = duration,
            Category = category,
            CreatedAt =  TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok"),
            IsActive = true
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
}
