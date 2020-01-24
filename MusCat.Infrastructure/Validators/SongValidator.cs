using FluentValidation;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Data;

namespace MusCat.Infrastructure.Validators
{
    public class SongValidator : AbstractValidator<Song>
    {
        public SongValidator()
        {
            RuleFor(song => song.Name)
                .Must(Be.NotEmpty)
                .WithMessage("The name must be not empty!")
                .MaximumLength(MusCatDbContext.SongNameMaxLength)
                .WithMessage($"The name must contain not more than {MusCatDbContext.SongNameMaxLength} symbols!"); ;

            RuleFor(song => song.TimeLength)
                .Matches(@"^\d+:\d{2}$")
                .WithMessage("Song duration should be in the format mm:ss");
        }
    }
}
