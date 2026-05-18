using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace store.Domain.Entities;

public class Movie
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; private set; } = ObjectId.GenerateNewId().ToString();
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public bool IsActive { get; private set; }

    private Movie() {}

    public static Movie Create(string name, decimal price, int stock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Pls named that movie");
        }

        if (price <= 0)
        {
            throw new ArgumentException("That price must more than 0");
        }

        if (stock < 0)
        {
            throw new ArgumentException("Stock cannot be negative.");
        }

        return new Movie
        {
            Name = name.Trim(),
            Price = price,
            Stock = stock,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };
    }

    public void Update(string? newName, decimal? newPrice)
    {
        // ถ้าส่งชื่อใหม่มา (ไม่เป็น null หรือค่าว่าง) ให้แทนที่ชื่อเดิม
        if (!string.IsNullOrWhiteSpace(newName))
        {
            Name = newName.Trim();
        }

        // ถ้าส่งราคาใหม่มา และราคามากกว่า 0 ให้แทนที่ราคาเดิม
        if (newPrice.HasValue && newPrice.Value > 0)
        {
            Price = newPrice.Value;
        }
    }

}
