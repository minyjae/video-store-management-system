using Microsoft.EntityFrameworkCore;
using store.Domain.Entities;
using store.Domain.Interfaces;
using store.Infrastructure.Data;

namespace store.Infrastructure.Repositories;

public class BannerRepository : IBannerRepository
{
    private readonly AppDbContext _db;

    public BannerRepository(AppDbContext db) => _db = db;

    public async Task<IEnumerable<Banner>> GetAllAsync() =>
        await _db.Banners
            .Where(b => !b.IsDeleted)
            .OrderBy(b => b.DisplayOrder)
            .ToListAsync();

    public async Task<Banner?> GetByIdAsync(Guid id) =>
        await _db.Banners.FindAsync(id);

    public async Task<int> CountAsync() =>
        await _db.Banners.CountAsync(b => !b.IsDeleted);

    public async Task AddAsync(Banner banner)
    {
        _db.Banners.Add(banner);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Banner banner)
    {
        _db.Banners.Update(banner);
        await _db.SaveChangesAsync();
    }
}
