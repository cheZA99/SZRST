using FluentValidation;
using SZRST.API.Controllers;

namespace SZRST.Web.Validator
{
    public class CurrencyCreateDtoValidator
        : AbstractValidator<CurrencyController.CurrencyCreateDto>
    {
        public CurrencyCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Naziv valute je obavezan.")
                .MaximumLength(100)
                .WithMessage("Naziv valute ne može biti duži od 100 karaktera.");

            RuleFor(x => x.ShortName)
                .NotEmpty()
                .WithMessage("Skraćeni naziv valute je obavezan.")
                .MaximumLength(10)
                .WithMessage("Skraćeni naziv ne može biti duži od 10 karaktera.");
        }
    }
}
