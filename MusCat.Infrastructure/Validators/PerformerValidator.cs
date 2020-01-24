using FluentValidation;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Data;

namespace MusCat.Infrastructure.Validators
{
    public class PerformerValidator : AbstractValidator<Performer>
    {
        public PerformerValidator()
        {
            RuleFor(performer => performer.Name)
                .Must(Be.NotEmpty)
                .WithMessage("The name must be not empty!")
                .MaximumLength(MusCatDbContext.PerformerNameMaxLength)
                .WithMessage($"The name must contain not more than {MusCatDbContext.PerformerNameMaxLength} symbols!");
        }
    }
}
