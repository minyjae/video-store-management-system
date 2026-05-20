// store.Application/Validators/DepositValidator.cs
using FluentValidation;
using store.Application.DTOs;

namespace store.Application.Validators;

public class DepositValidator : AbstractValidator<DepositDto>
{
    public DepositValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Deposit amount must be positive.")
            .LessThanOrEqualTo(100000).WithMessage("Max deposit is 100,000.");
    }
}