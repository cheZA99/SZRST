using FluentValidation;
using SZRST.API.Controllers;

namespace SZRST.Web.Validator
{
    public class CountryCreateDtoValidator : AbstractValidator<CountryCreateDto>
    {
        public CountryCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Naziv države je obavezan.")
                .MaximumLength(150)
                .WithMessage("Naziv države ne može biti duži od 150 karaktera.");

            RuleFor(x => x.ShortName)
                .NotEmpty()
                .WithMessage("Skraćeni naziv države je obavezan.")
                .MaximumLength(10)
                .WithMessage("Skraćeni naziv ne može biti duži od 10 karaktera.");

            RuleFor(x => x.CurrencyId)
                .GreaterThan(0)
                .When(x => x.CurrencyId.HasValue)
                .WithMessage("CurrencyId mora biti validan.");
        }
    }
}
