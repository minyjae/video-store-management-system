using FluentValidation;
using Store.Application.DTOs;

namespace Store.Application.Validators;

// ต้องสืบทอดจาก AbstractValidator<T>
public class CreateMovieValidator : AbstractValidator<CreateMovieDto>
{
    public CreateMovieValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Movie name is required.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Movie price must be greater than 0.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Movie stock cannot be negative.");
    }
}