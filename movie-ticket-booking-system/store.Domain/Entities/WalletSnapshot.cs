// store.Domain/Entities/WalletSnapshot.cs
namespace store.Domain.Entities;

public class WalletSnapshot
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public decimal Balance { get; private set; }
    public Guid LastEntryId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public User? User { get; private set; }

    private WalletSnapshot() {}

    public static WalletSnapshot Create(Guid userId, decimal balance, Guid lastEntryId)
    {
        return new WalletSnapshot
        {
            UserId      = userId,
            Balance     = balance,
            LastEntryId = lastEntryId,
            CreatedAt   = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok")
        };
    }
}