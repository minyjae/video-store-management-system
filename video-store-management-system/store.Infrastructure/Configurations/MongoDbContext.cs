using MongoDB.Driver;

namespace store.Infrastructure.Data;

public class MongoDbContext
{
    private readonly IMongoDatabase _database;

    public MongoDbContext(IConfiguration configuration)
    {
        var username = Environment.GetEnvironmentVariable("MONGO_USERNAME");
        var password = Environment.GetEnvironmentVariable("MONGO_PASSWORD");
        var port     = Environment.GetEnvironmentVariable("MONGO_PORT");
        var database = Environment.GetEnvironmentVariable("MONGO_DATABASE");

        var connectionString = $"mongodb://{username}:{password}@localhost:{port}";

        var client = new MongoClient(connectionString);
        _database = client.GetDatabase(database);
    }

    public IMongoCollection<T> GetCollection<T>(string name)
        => _database.GetCollection<T>(name);
}