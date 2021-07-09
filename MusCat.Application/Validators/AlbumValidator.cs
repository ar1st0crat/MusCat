using FluentValidation;
using MusCat.Core.Entities;

namespace MusCat.Application.Validators
{
    public class AlbumValidator : AbstractValidator<Album>
    {
        public static int AlbumNameMaxLength { get; set; } = 50;

        public AlbumValidator()
        {
            RuleFor(album => album.Name)
                .Must(Be.NotEmpty)
                .WithMessage("The name must be not empty!")
                .MaximumLength(AlbumNameMaxLength)
                .WithMessage($"The name must contain not more than {AlbumNameMaxLength} symbols!");

            RuleFor(album => album.TotalTime)
                .Matches(@"^\d+:\d{2}$")
                .WithMessage("Total time should be in the format mm:ss");

            RuleFor(album => album.ReleaseYear)
                .Must(x => x >= 1000 && x <= 9999)
                .WithMessage($"The year of release must contain four digits!");
        }
    }
}
