using FluentValidation;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Data;

namespace MusCat.Infrastructure.Validators
{
    public class CountryValidator : AbstractValidator<Country>
    {
        public CountryValidator()
        {
            RuleFor(country => country.Name)
                .Must(Be.NotEmpty)
                .WithMessage("The name must be not empty!")
                .MaximumLength(MusCatDbContext.CountryNameMaxLength)
                .WithMessage($"The name must contain not more than {MusCatDbContext.CountryNameMaxLength} symbols!");
        }
    }
}
