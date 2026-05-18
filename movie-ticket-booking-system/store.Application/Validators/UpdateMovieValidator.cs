using FluentValidation;
using store.Application.DTOs;

namespace store.Application.Validators;

public class UpdateMovieValidator : AbstractValidator<UpdateMovieDto>
{
    public UpdateMovieValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Movie Id is required.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title must not be empty.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.")
            .When(x => x.Title != null);

        RuleFor(x => x.Plot)
            .NotEmpty().WithMessage("Plot must not be empty.")
            .MaximumLength(1000).WithMessage("Plot must not exceed 1000 characters.")
            .When(x => x.Plot != null);

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .When(x => x.Price.HasValue);

        RuleFor(x => x.Duration)
            .GreaterThan(TimeSpan.Zero).WithMessage("Duration must be greater than 0.")
            .LessThanOrEqualTo(TimeSpan.FromHours(10)).WithMessage("Duration must not exceed 10 hours.")
            .When(x => x.Duration.HasValue);

        RuleFor(x => x.Category)
            .IsInEnum().WithMessage("Invalid movie category.")
            .When(x => x.Category.HasValue);
    }
}