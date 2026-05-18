using FluentValidation;
using store.Application.DTOs;

namespace store.Application.Validators;

public class CreateMovieValidator : AbstractValidator<CreateMovieDto>
{
    public CreateMovieValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Movie title is required.")
            .MaximumLength(200).WithMessage("Movie title must not exceed 200 characters.");

        RuleFor(x => x.Plot)
            .NotEmpty().WithMessage("Movie plot is required.")
            .MaximumLength(1000).WithMessage("Movie plot must not exceed 1000 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Movie price must be greater than 0.");

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Movie duration must be greater than 0.")
            .LessThanOrEqualTo(TimeSpan.FromHours(10)).WithMessage("Movie duration must not exceed 10 hours.");

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid movie category.");
    }
}