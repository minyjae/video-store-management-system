using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Application.DTOs;
using store.Application.Interfaces;

namespace store.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BannersController : ControllerBase
{
    private readonly IBannerService _bannerService;
    private readonly IWebHostEnvironment _env;

    public BannersController(IBannerService bannerService, IWebHostEnvironment env)
    {
        _bannerService = bannerService;
        _env = env;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var banners = await _bannerService.GetAllAsync();
        return Ok(banners);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromForm] CreateBannerDto dto, [FromForm] IFormFile image)
    {
        var banner = await _bannerService.CreateAsync(dto, image, _env.WebRootPath);
        return Ok(banner);
    }

    [HttpPut]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update([FromBody] UpdateBannerDto dto)
    {
        var banner = await _bannerService.UpdateAsync(dto);
        return Ok(banner);
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _bannerService.DeleteAsync(id);
        return NoContent();
    }
}
