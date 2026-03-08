using FluentValidation;
using SZRST.API.Controllers;

namespace SZRST.Web.Validator
{
    public class FacilityFilterDtoValidator : AbstractValidator<FacilityFilterDto>
    {
        public FacilityFilterDtoValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0);

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100);

            RuleFor(x => x.SortDirection)
                .Must(x => x == null || x == "asc" || x == "desc")
                .WithMessage("SortDirection mora biti 'asc' ili 'desc'");
        }
    }
}
