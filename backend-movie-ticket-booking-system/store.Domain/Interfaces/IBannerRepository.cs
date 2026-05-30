using store.Domain.Entities;

namespace store.Domain.Interfaces;

public interface IBannerRepository
{
    Task<IEnumerable<Banner>> GetAllAsync();
    Task<Banner?> GetByIdAsync(Guid id);
    Task<int> CountAsync();
    Task AddAsync(Banner banner);
    Task UpdateAsync(Banner banner);
}
