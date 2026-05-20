// store.Application/Interfaces/IWalletService.cs
using store.Application.DTOs;

namespace store.Application.Interfaces;

public interface IWalletService
{
    Task<decimal> GetBalanceAsync(Guid userId);
    Task DepositAsync(Guid userId, decimal amount);
}