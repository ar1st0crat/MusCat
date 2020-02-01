using FluentValidation;
using MusCat.Core.Entities;

namespace MusCat.Application.Validators
{
    public class PerformerValidator : AbstractValidator<Performer>
    {
        public static int PerformerNameMaxLength { get; set; } = 30;

        public PerformerValidator()
        {
            RuleFor(performer => performer.Name)
                .Must(Be.NotEmpty)
                .WithMessage("The name must be not empty!")
                .MaximumLength(PerformerNameMaxLength)
                .WithMessage($"The name must contain not more than {PerformerNameMaxLength} symbols!");
        }
    }
}
