using FluentValidation;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Linq;
using SZRST.API.Controllers;

namespace SZRST.Web.Validator
{
    public class FacilityLocationCreateWithImageDtoValidator
        : AbstractValidator<FacilityLocationCreateWithImageDto>
    {
        public FacilityLocationCreateWithImageDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Naziv objekta je obavezan")
                .MaximumLength(200);

            RuleFor(x => x.FacilityTypeId)
                .GreaterThan(0)
                .WithMessage("FacilityTypeId mora biti validan");

            RuleFor(x => x.CountryId)
                .GreaterThan(0)
                .WithMessage("CountryId mora biti validan");

            RuleFor(x => x.CityId)
                .GreaterThan(0)
                .WithMessage("CityId mora biti validan");

            RuleFor(x => x.Address)
                .NotEmpty().WithMessage("Adresa je obavezna")
                .MaximumLength(200);

            RuleFor(x => x.AddressNumber)
                .NotEmpty().WithMessage("Broj adrese je obavezan")
                .MaximumLength(20);

            RuleFor(x => x.TenantId)
                .GreaterThan(0)
                .WithMessage("TenantId mora biti validan");

            RuleFor(x => x.File)
                .Must(BeValidImage)
                .When(x => x.File != null)
                .WithMessage("Dozvoljeni formati su: jpg, jpeg, png, webp");
        }

        private bool BeValidImage(IFormFile file)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLower();
            return allowed.Contains(extension);
        }
    }
}
