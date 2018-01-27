using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;

namespace MusCat.Core.Services
{
    public class AlbumService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlbumService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public Result<Album> AddAlbum(string name)
        {
            var album = new Album { Name = name };

            if (album.Error != string.Empty)
            {
                return new Result<Album>(ResultType.Invalid, album.Error);
            }

            _unitOfWork.AlbumRepository.Add(album);
            _unitOfWork.Save();

            return new Result<Album>(album);
        }

        public Result<Album> RemoveAlbum(long albumId)
        {
            var album = _unitOfWork.AlbumRepository
                                   .Get(a => a.Id == albumId)
                                   .FirstOrDefault();
            if (album == null)
            {
                return new Result<Album>(ResultType.Invalid, "Could not find album!");
            }

            _unitOfWork.AlbumRepository.Delete(album);
            _unitOfWork.Save();

            return new Result<Album>(album);
        }

        public Result<Album> UpdateAlbum(long albumId, string name)
        {
            var album = new Album { Name = name };

            if (album.Error != string.Empty)
            {
                return new Result<Album>(ResultType.Invalid, album.Error);
            }

            album = _unitOfWork.AlbumRepository
                               .Get(a => a.Id == albumId)
                               .FirstOrDefault();

            if (album == null)
            {
                return new Result<Album>(ResultType.Invalid, "Could not find album!");
            }

            album.Name = name;
            _unitOfWork.AlbumRepository.Edit(album);
            _unitOfWork.Save();

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

        public async Task<IEnumerable<Song>> LoadAlbumSongsAsync(long albumId)
        {
            var album = _unitOfWork.AlbumRepository
                                   .Get(a => a.Id == albumId)
                                   .FirstOrDefault();

            return await _unitOfWork.AlbumRepository
                                    .GetAlbumSongsAsync(album)
                                    .ConfigureAwait(false);
        }
    }
}
