using FluentValidation;
using MusCat.Core.Entities;

namespace MusCat.Application.Validators
{
    public class SongValidator : AbstractValidator<Song>
    {
        public static int SongNameMaxLength { get; set; } = 50;

        public SongValidator()
        {
            RuleFor(song => song.Name)
                .Must(Be.NotEmpty)
                .WithMessage("The name must be not empty!")
                .MaximumLength(SongNameMaxLength)
                .WithMessage($"The name must contain not more than {SongNameMaxLength} symbols!"); ;

            RuleFor(song => song.TimeLength)
                .Matches(@"^\d+:\d{2}$")
                .WithMessage("Song duration should be in the format mm:ss");
        }
    }
}
