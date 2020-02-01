using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Util;
using MusCat.Application.Dto;
using MusCat.Application.Interfaces;
using MusCat.Application.Validators;

namespace MusCat.Application.Services
{
    public class SongService : ISongService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SongService(IUnitOfWork unitOfWork)
        {
            Guard.AgainstNull(unitOfWork);
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<SongDto>> AddSongAsync(Song song)
        {
            var result = new SongValidator().Validate(song);

            if (!result.IsValid)
            {
                return new Result<SongDto>(ResultType.Invalid, string.Join("; ", result.Errors));
            }

            _unitOfWork.SongRepository.Add(song);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<SongDto>(Auto.Mapper.Map<SongDto>(song));
        }

        public async Task<Result<SongDto>> RemoveSongAsync(int songId)
        {
            var songs = await _unitOfWork.SongRepository
                                         .GetAsync(c => c.Id == songId)
                                         .ConfigureAwait(false);

            var song = songs.FirstOrDefault();

            if (song == null)
            {
                return new Result<SongDto>(ResultType.Invalid, "Could not find Song!");
            }

            _unitOfWork.SongRepository.Delete(song);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<SongDto>(Auto.Mapper.Map<SongDto>(song));
        }

        public async Task<Result<SongDto>> UpdateSongAsync(Song newSong)
        {
            var result = new SongValidator().Validate(newSong);

            if (!result.IsValid)
            {
                return new Result<SongDto>(ResultType.Invalid, string.Join("; ", result.Errors));
            }

            var song = _unitOfWork.SongRepository
                                  .Get(c => c.Id == newSong.Id)
                                  .FirstOrDefault();

            if (song == null)
            {
                return new Result<SongDto>(ResultType.Invalid, "Could not find Song!");
            }

            song.TrackNo = newSong.TrackNo;
            song.Name = newSong.Name;
            song.TimeLength = newSong.TimeLength;
            song.Rate = newSong.Rate;
            
            _unitOfWork.SongRepository.Edit(song);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<SongDto>(Auto.Mapper.Map<SongDto>(song));
        }
    }
}
