using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace store.Domain.Entities;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id {get; private set;} = ObjectId.GenerateNewId().ToString();
    public string Username { get; private set;} = string.Empty;
    public string HashedPassword { get; private set; } = string.Empty;
    public List<Movie> Wishlist { get; private set; }

    private User() {}

    public static User Register(string username, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.");

        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Password is required.");

        return new User
        {
            Username = username.Trim(),
            HashedPassword = hashedPassword
        };
    }
}