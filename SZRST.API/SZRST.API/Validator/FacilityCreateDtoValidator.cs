using FluentValidation;
using SZRST.API.Controllers;

namespace SZRST.Web.Validator
{
    public class FacilityCreateDtoValidator : AbstractValidator<FacilityCreateDto>
    {
        public FacilityCreateDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naziv objekta je obavezan")
                .MaximumLength(200);

            RuleFor(x => x.FacilityTypeId)
                .GreaterThan(0)
                .WithMessage("FacilityTypeId mora biti validan");

            RuleFor(x => x.LocationId)
                .GreaterThan(0)
                .WithMessage("LocationId mora biti validan");

            RuleFor(x => x.ImageUrl)
                .MaximumLength(500)
                .When(x => !string.IsNullOrWhiteSpace(x.ImageUrl));
        }
    }
}
