using store.Application.DTOs;
using store.Application.Interfaces;
using store.Domain.Entities;
using store.Domain.Interfaces;

namespace store.Application.Services;

public class BannerService : IBannerService
{
    private readonly IBannerRepository _repo;

    public BannerService(IBannerRepository repo) => _repo = repo;

    public async Task<IEnumerable<BannerDto>> GetAllAsync()
    {
        var banners = await _repo.GetAllAsync();
        return banners.OrderBy(b => b.DisplayOrder).Select(MapToDto);
    }

    public async Task<BannerDto> CreateAsync(CreateBannerDto dto, IFormFile image, string webRootPath)
    {
        var ext = Path.GetExtension(image.FileName).ToLowerInvariant();
        var fileName = $"{Guid.NewGuid()}{ext}";
        var uploadPath = Path.Combine(webRootPath, "uploads", "banners");
        Directory.CreateDirectory(uploadPath);
        var filePath = Path.Combine(uploadPath, fileName);

        await using (var stream = File.Create(filePath))
            await image.CopyToAsync(stream);

        var count = await _repo.CountAsync();
        var banner = Banner.Create($"/uploads/banners/{fileName}", dto.Title, dto.Tagline, dto.Genre, count + 1);

        try
        {
            await _repo.AddAsync(banner);
        }
        catch
        {
            if (File.Exists(filePath)) File.Delete(filePath);
            throw;
        }

        return MapToDto(banner);
    }

    public async Task<BannerDto> UpdateAsync(UpdateBannerDto dto)
    {
        var banner = await _repo.GetByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException($"Banner {dto.Id} not found.");

        banner.Update(dto.Title, dto.Tagline, dto.Genre);
        await _repo.UpdateAsync(banner);

        return MapToDto(banner);
    }

    public async Task DeleteAsync(Guid id)
    {
        var banner = await _repo.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Banner {id} not found.");

        banner.SoftDelete();
        await _repo.UpdateAsync(banner);
    }

    private static BannerDto MapToDto(Banner b) =>
        new(b.Id, b.ImageUrl, b.Title, b.Tagline, b.Genre, b.DisplayOrder, b.CreatedAt);
}
