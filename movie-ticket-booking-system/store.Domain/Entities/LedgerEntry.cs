// store.Domain/Entities/LedgerEntry.cs
using store.Domain.Enums;

namespace store.Domain.Entities;

public class LedgerEntry
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    public Guid UserId { get; private set; }
    public decimal Amount { get; private set; }
    public LedgerEntryType Type { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public Guid? ReferenceId { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation Property
    public User? User { get; private set; }

    private LedgerEntry() {}

    public static LedgerEntry CreateDeposit(Guid userId, decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Deposit amount must be positive.");

        return new LedgerEntry
        {
            UserId      = userId,
            Amount      = amount,
            Type        = LedgerEntryType.Deposit,
            Description = $"Deposit {amount:C}",
            CreatedAt   = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok")
        };
    }

    public static LedgerEntry CreateTicketPurchase(Guid userId,
                                                    decimal amount,
                                                    Guid ticketId)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.");

        return new LedgerEntry
        {
            UserId      = userId,
            Amount      = -amount,
            Type        = LedgerEntryType.TicketPurchase,
            Description = $"Ticket purchase {amount:C}",
            ReferenceId = ticketId,
            CreatedAt   = TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok")
        };
    }
}