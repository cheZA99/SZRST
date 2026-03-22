using FluentValidation;
using SZRST.Web.Controllers;

namespace SZRST.Web.Validator
{
    public class ReservationReportRequestValidator
        : AbstractValidator<AppointmentReportRequest>
    {
        public ReservationReportRequestValidator()
        {
            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("TenantId mora biti validan.");

            RuleFor(x => x.DateFrom)
                .NotEmpty()
                .WithMessage("DateFrom je obavezan datum.");

            RuleFor(x => x.DateTo)
                .NotEmpty()
                .WithMessage("DateTo je obavezan datum.");

            RuleFor(x => x)
                .Must(x => x.DateTo >= x.DateFrom)
                .WithMessage("DateTo mora biti veći ili jednak DateFrom.");

            RuleFor(x => x)
                .Must(x => (x.DateTo - x.DateFrom).TotalDays <= 365)
                .WithMessage("Period izvještaja ne može biti duži od 365 dana.");
        }
    }
}
