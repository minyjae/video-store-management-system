// store.Domain/Entities/User.cs
using store.Domain.Enums;

namespace store.Domain.Entities;

public class User
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public string Username { get; private set; } = string.Empty;
    public string HashedPassword { get; private set; } = string.Empty;
    public UserRole Role { get; private set; } = UserRole.User;

    // Navigation Properties
    public List<LedgerEntry> LedgerEntries { get; private set; } = new();
    public List<Ticket> Tickets { get; private set; } = new();

    private User() {}

    public static User Register(string username, string hashedPassword, UserRole role = UserRole.User)
    {
        if (string.IsNullOrWhiteSpace(username))
            throw new ArgumentException("Username is required.");
        if (string.IsNullOrWhiteSpace(hashedPassword))
            throw new ArgumentException("Password is required.");

        return new User
        {
            Username       = username.Trim(),
            HashedPassword = hashedPassword,
            Role           = role
        };
    }
}