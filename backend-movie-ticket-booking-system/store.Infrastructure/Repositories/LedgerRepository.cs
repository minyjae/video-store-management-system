// store.Infrastructure/Repositories/LedgerRepository.cs
using Microsoft.EntityFrameworkCore;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Infrastructure.Repositories;

public class LedgerRepository : ILedgerRepository
{
    private readonly AppDbContext _context;

    public LedgerRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        var snapshot = await GetLatestSnapshotAsync(userId);

        if (snapshot is null)
            return await SumAllEntriesAsync(userId);

        var additional = await GetBalanceAfterSnapshotAsync(userId, snapshot.LastEntryId);
        return snapshot.Balance + additional;
    }

    private async Task<decimal> SumAllEntriesAsync(Guid userId)
        => await _context.LedgerEntries
            .Where(e => e.UserId == userId)
            .SumAsync(e => e.Amount);

    public async Task<decimal> GetBalanceAfterSnapshotAsync(Guid userId, Guid lastEntryId)
    {
        var lastEntry = await _context.LedgerEntries.FindAsync(lastEntryId);
        if (lastEntry is null) return 0m;

        return await _context.LedgerEntries
            .Where(e => e.UserId == userId && e.CreatedAt > lastEntry.CreatedAt)
            .SumAsync(e => e.Amount);
    }

    public async Task<int> CountEntriesAfterSnapshotAsync(Guid userId)
    {
        var snapshot = await GetLatestSnapshotAsync(userId);

        if (snapshot is null)
            return await _context.LedgerEntries
                .CountAsync(e => e.UserId == userId);

        var lastEntry = await _context.LedgerEntries.FindAsync(snapshot.LastEntryId);
        if (lastEntry is null) return 0;

        return await _context.LedgerEntries
            .CountAsync(e => e.UserId == userId && e.CreatedAt > lastEntry.CreatedAt);
    }

    public async Task<Guid?> GetLastEntryIdAsync(Guid userId)
    {
        var last = await _context.LedgerEntries
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .FirstOrDefaultAsync();
        return last?.Id;
    }

    public async Task AppendAsync(LedgerEntry entry)
    {
        await _context.LedgerEntries.AddAsync(entry);
        // ไม่ SaveChanges — ให้ caller จัดการ (Transaction หรือ SaveAsync)
    }

    public async Task SaveAsync()
        => await _context.SaveChangesAsync();

    public async Task<WalletSnapshot?> GetLatestSnapshotAsync(Guid userId)
        => await _context.WalletSnapshots
            .Where(s => s.UserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .FirstOrDefaultAsync();

    public async Task SaveSnapshotAsync(WalletSnapshot snapshot)
    {
        await _context.WalletSnapshots.AddAsync(snapshot);
        await _context.SaveChangesAsync();
    }
}