using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Domain;
using MusCat.Core.Util;
using System.Linq;
using System.Threading.Tasks;

namespace MusCat.Core.Services
{
    public class SongService : ISongService
    {
        private readonly IUnitOfWork _unitOfWork;

        public SongService(IUnitOfWork unitOfWork)
        {
            Guard.AgainstNull(unitOfWork);
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Song>> AddSongAsync(Song Song)
        {
            if (Song.Error != string.Empty)
            {
                return new Result<Song>(ResultType.Invalid, Song.Error);
            }

            _unitOfWork.SongRepository.Add(Song);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Song>(Song);
        }

        public async Task<Result<Song>> RemoveSongAsync(int SongId)
        {
            var Songs = await _unitOfWork.SongRepository
                                         .GetAsync(c => c.Id == SongId)
                                         .ConfigureAwait(false);

            var Song = Songs.FirstOrDefault();

            if (Song == null)
            {
                return new Result<Song>(ResultType.Invalid, "Could not find Song!");
            }

            _unitOfWork.SongRepository.Delete(Song);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Song>(Song);
        }

        public async Task<Result<Song>> UpdateSongAsync(Song newSong)
        {
            if (newSong.Error != string.Empty)
            {
                return new Result<Song>(ResultType.Invalid, newSong.Error);
            }

            var song = _unitOfWork.SongRepository
                                  .Get(c => c.Id == newSong.Id)
                                  .FirstOrDefault();

            if (song == null)
            {
                return new Result<Song>(ResultType.Invalid, "Could not find Song!");
            }

            song.TrackNo = newSong.TrackNo;
            song.Name = newSong.Name;
            song.TimeLength = newSong.TimeLength;
            song.Rate = newSong.Rate;
            
            _unitOfWork.SongRepository.Edit(song);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Song>(song);
        }
    }
}
