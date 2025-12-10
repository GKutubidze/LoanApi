using FluentValidation;
using LoanApi.DTOs;

namespace LoanApi.Validators;

public class LoanCreateDtoValidator : AbstractValidator<LoanCreateDto>
{
    public LoanCreateDtoValidator()
    {
        RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Loan amount must be greater than 0");

        RuleFor(x => x.Currency)
            .Matches("^[A-Z]{3}$")
            .WithMessage("Currency must be a valid 3-letter ISO code (e.g., USD, EUR, GEL)");


        RuleFor(x => x.LoanPeriod)
            .GreaterThan(0).WithMessage("Loan period must be greater than 0 months");

        RuleFor(x => x.LoanType)
            .IsInEnum().WithMessage("Invalid loan type");
    }
}