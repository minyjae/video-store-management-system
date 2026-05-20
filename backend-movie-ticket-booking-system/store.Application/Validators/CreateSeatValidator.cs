// store.Application/Validators/CreateSeatValidator.cs
using FluentValidation;
using store.Application.DTOs;

namespace store.Application.Validators;

public class CreateSeatValidator : AbstractValidator<CreateSeatDto>
{
    public CreateSeatValidator()
    {
        RuleFor(x => x.ShowtimeId)
            .NotEmpty().WithMessage("ShowtimeId is required.");

        RuleFor(x => x.SeatCode)
            .NotEmpty().WithMessage("SeatCode is required.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}