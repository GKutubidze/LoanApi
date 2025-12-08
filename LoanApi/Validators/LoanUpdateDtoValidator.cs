using FluentValidation;
using LoanApi.DTOs;

namespace LoanApi.Validators;

public class LoanUpdateDtoValidator : AbstractValidator<LoanUpdateDto>
{
    public LoanUpdateDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Loan amount must be greater than 0");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required")
            .MaximumLength(10).WithMessage("Currency code must not exceed 10 characters");

        RuleFor(x => x.LoanPeriod)
            .GreaterThan(0).WithMessage("Loan period must be greater than 0 months");

        RuleFor(x => x.LoanType)
            .IsInEnum().WithMessage("Invalid loan type");
    }
}