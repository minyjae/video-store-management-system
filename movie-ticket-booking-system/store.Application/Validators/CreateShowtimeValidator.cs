// store.Application/Validators/CreateShowtimeValidator.cs
using FluentValidation;
using store.Application.DTOs;

namespace store.Application.Validators;

public class CreateShowtimeValidator : AbstractValidator<CreateShowtimeDto>
{
    public CreateShowtimeValidator()
    {
        RuleFor(x => x.MovieId)
            .NotEmpty().WithMessage("MovieId is required.");

        RuleFor(x => x.ScreenId)
            .NotEmpty().WithMessage("ScreenId is required.");

        RuleFor(x => x.StartTime)
            .GreaterThan(TimeZoneInfo.ConvertTimeBySystemTimeZoneId(DateTime.UtcNow, "Asia/Bangkok")).WithMessage("StartTime must be in the future.");
    }
}