using FluentValidation;
using MusCat.Core.Entities;

namespace MusCat.Application.Validators
{
    public class CountryValidator : AbstractValidator<Country>
    {
        public static int CountryNameMaxLength { get; set; } = 20;

        public CountryValidator()
        {
            RuleFor(country => country.Name)
                .Must(Be.NotEmpty)
                .WithMessage("The name must be not empty!")
                .MaximumLength(CountryNameMaxLength)
                .WithMessage($"The name must contain not more than {CountryNameMaxLength} symbols!");
        }
    }
}
