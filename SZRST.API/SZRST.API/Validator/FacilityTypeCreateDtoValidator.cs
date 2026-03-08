using FluentValidation;
using SZRST.API.Controllers;

namespace SZRST.Web.Validator
{
    public class FacilityTypeCreateDtoValidator : AbstractValidator<FacilityTypeCreateDto>
    {
        public FacilityTypeCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .WithMessage("Naziv tipa objekta je obavezan.")
                .MaximumLength(150)
                .WithMessage("Naziv ne može biti duži od 150 karaktera.");

            RuleFor(x => x.Description)
                .MaximumLength(500)
                .WithMessage("Opis ne može biti duži od 500 karaktera.")
                .When(x => !string.IsNullOrWhiteSpace(x.Description));
        }
    }
}
