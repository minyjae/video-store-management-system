using Microsoft.AspNetCore.Http;
using store.Application.DTOs;

namespace store.Application.Interfaces;

public interface IBannerService
{
    Task<IEnumerable<BannerDto>> GetAllAsync();
    Task<BannerDto> CreateAsync(CreateBannerDto dto, IFormFile image, string webRootPath);
    Task<BannerDto> UpdateAsync(UpdateBannerDto dto);
    Task DeleteAsync(Guid id);
}
