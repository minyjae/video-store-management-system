// store.Domain/Interfaces/ILedgerRepository.cs
using store.Domain.Entities;

namespace store.Domain.Interfaces;

public interface ILedgerRepository
{
    Task<decimal> GetBalanceAsync(Guid userId);
    Task AppendAsync(LedgerEntry entry);
    Task SaveAsync();                        // สำหรับ standalone operations ที่ไม่มี outer Transaction
    Task<WalletSnapshot?> GetLatestSnapshotAsync(Guid userId);
    Task<decimal> GetBalanceAfterSnapshotAsync(Guid userId, Guid lastEntryId);
    Task<int> CountEntriesAfterSnapshotAsync(Guid userId);
    Task<Guid?> GetLastEntryIdAsync(Guid userId);
    Task SaveSnapshotAsync(WalletSnapshot snapshot);
}