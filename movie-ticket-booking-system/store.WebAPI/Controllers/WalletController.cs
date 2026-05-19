// store.WebAPI/Controllers/WalletController.cs
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using store.Application.DTOs;
using store.Application.Interfaces;
using System.Security.Claims;

namespace store.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]                             // ต้อง Login ก่อนทุก Endpoint
public class WalletController : ControllerBase
{
    private readonly IWalletService _walletService;
    private readonly IValidator<DepositDto> _depositValidator;

    public WalletController(
        IWalletService walletService,
        IValidator<DepositDto> depositValidator)
    {
        _walletService = walletService;
        _depositValidator = depositValidator;
    }

    [HttpGet("balance")]                // GET /api/wallet/balance
    public async Task<IActionResult> GetBalance()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not found in token.");
        var userGuid = Guid.Parse(userId);

        var balance = await _walletService.GetBalanceAsync(userGuid);
        return Ok(new WalletDto(userGuid, balance));
    }

    [HttpPost("deposit")]               // POST /api/wallet/deposit
    public async Task<IActionResult> Deposit([FromBody] DepositDto dto)
    {
        var validation = await _depositValidator.ValidateAsync(dto);
        if (!validation.IsValid)
            return BadRequest(validation.Errors.Select(e => e.ErrorMessage));

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User not found in token.");
        var userGuid = Guid.Parse(userId);

        await _walletService.DepositAsync(userGuid, dto.Amount);

        var balance = await _walletService.GetBalanceAsync(userGuid);
        return Ok(new WalletDto(userGuid, balance));
    }
}