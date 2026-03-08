using FluentValidation;
using SZRST.API.Controllers;

namespace SZRST.Web.Validator
{
    public class CityCreateDtoValidator : AbstractValidator<CityCreateDto>
    {
        public CityCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Naziv grada je obavezan.")
                .MaximumLength(150)
                .WithMessage("Naziv grada ne može biti duži od 150 karaktera.");

            RuleFor(x => x.CountryId)
                .GreaterThan(0)
                .WithMessage("CountryId mora biti validan.");
        }
    }
}
