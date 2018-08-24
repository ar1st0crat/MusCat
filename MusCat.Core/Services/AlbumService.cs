using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Domain;
using MusCat.Core.Util;

namespace MusCat.Core.Services
{
    public class AlbumService : IAlbumService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlbumService(IUnitOfWork unitOfWork)
        {
            Guard.AgainstNull(unitOfWork);
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Album>> AddAlbumAsync(Album album)
        {
            if (album.Error != string.Empty)
            {
                return new Result<Album>(ResultType.Invalid, album.Error);
            }

            await _unitOfWork.AlbumRepository.AddAsync(album).ConfigureAwait(false);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Album>(album);
        }

        public async Task<Result<Album>> RemoveAlbumAsync(long albumId)
        {
            var albums = await _unitOfWork.AlbumRepository
                                         .GetAsync(a => a.Id == albumId)
                                         .ConfigureAwait(false);

            var album = albums.FirstOrDefault();

            if (album == null)
            {
                return new Result<Album>(ResultType.Invalid, "Could not find album!");
            }

            _unitOfWork.AlbumRepository.Delete(album);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Album>(album);
        }

        public async Task<Result<Album>> UpdateAlbumAsync(Album newAlbum)
        {
            if (newAlbum.Error != string.Empty)
            {
                return new Result<Album>(ResultType.Invalid, newAlbum.Error);
            }

            var albums = await _unitOfWork.AlbumRepository
                                          .GetAsync(a => a.Id == newAlbum.Id)
                                          .ConfigureAwait(false);

            var album = albums.FirstOrDefault();

            if (album == null)
            {
                return new Result<Album>(ResultType.Invalid, "Could not find album!");
            }

            album.Name = newAlbum.Name;
            album.TotalTime = newAlbum.TotalTime;
            album.ReleaseYear = newAlbum.ReleaseYear;
            album.Rate = newAlbum.Rate;

            _unitOfWork.AlbumRepository.Edit(album);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Album>(album);
        }

        public Result<Album> UpdateAlbumRate(long albumId, byte? rate)
        {
            var album = _unitOfWork.AlbumRepository
                                   .Get(a => a.Id == albumId)
                                   .FirstOrDefault();

            if (album == null)
            {
                return new Result<Album>(ResultType.Invalid, "Could not find album!");
            }

            album.Rate = rate;

            _unitOfWork.AlbumRepository.Edit(album);
            _unitOfWork.Save();

            return new Result<Album>(album);
        }

        /// <summary>
        /// This version loads all album songs including Album and Album.Performer fields
        /// </summary>
        public async Task<IEnumerable<Song>> LoadAlbumSongsAsync(long albumId)
        {
            var albums = await _unitOfWork.AlbumRepository
                                          .GetAsync(a => a.Id == albumId)
                                          .ConfigureAwait(false);

            var album = albums.FirstOrDefault();

            album.Performer = (await _unitOfWork.PerformerRepository
                                                .GetAsync(p => p.Id == album.PerformerId)
                                                .ConfigureAwait(false))
                                                .FirstOrDefault();

            return await _unitOfWork.SongRepository
                                    .GetAsync(s => s.AlbumId == album.Id)
                                    .ConfigureAwait(false);
        }
    }
}
