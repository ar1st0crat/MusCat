using FluentValidation;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Data;

namespace MusCat.Infrastructure.Validators
{
    public class AlbumValidator : AbstractValidator<Album>
    {
        public AlbumValidator()
        {
            RuleFor(album => album.Name)
                .Must(Be.NotEmpty)
                .WithMessage("The name must be not empty!")
                .MaximumLength(MusCatDbContext.AlbumNameMaxLength)
                .WithMessage($"The name must contain not more than {MusCatDbContext.AlbumNameMaxLength} symbols!");

            RuleFor(album => album.TotalTime)
                .Matches(@"^\d+:\d{2}$")
                .WithMessage("Total time should be in the format mm:ss");
        }
    }
}
