// store.Application/DTOs/WalletDto.cs
namespace store.Application.DTOs;

public record WalletDto(
    Guid UserId,
    decimal Balance
);

public record DepositDto(
    decimal Amount
);