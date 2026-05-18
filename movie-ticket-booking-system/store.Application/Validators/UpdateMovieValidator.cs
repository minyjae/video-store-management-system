using FluentValidation;
using store.Application.DTOs;

namespace store.Application.Validators;

public class UpdateMovieValidator : AbstractValidator<UpdateMovieDto>
{
    public UpdateMovieValidator()
    {
        // ตรวจสอบ Name: 
        // ทำงานเฉพาะเมื่อ Name ไม่ใช่ null
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name must not be empty.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.")
            .When(x => x.Name != null);

        // ตรวจสอบ Price: 
        // ทำงานเฉพาะเมื่อ Price มีการส่งค่ามา (HasValue)
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.")
            .When(x => x.Price.HasValue);
    }
}