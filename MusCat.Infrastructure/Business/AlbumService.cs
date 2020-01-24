using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Application.Dto;
using MusCat.Application.Interfaces;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Util;
using MusCat.Infrastructure.Validators;

namespace MusCat.Infrastructure.Business
{
    public class AlbumService : IAlbumService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AlbumService(IUnitOfWork unitOfWork)
        {
            Guard.AgainstNull(unitOfWork);
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<AlbumDto>> GetAllAlbumsAsync()
        {
            var albums = await _unitOfWork.AlbumRepository.GetAllAsync().ConfigureAwait(false);
            return Auto.Mapper.Map<IEnumerable<AlbumDto>>(albums); 
        }

        public async Task<Result<AlbumDto>> GetAlbumAsync(int albumId, bool loadPerformer = false)
        {
            var albums = await _unitOfWork.AlbumRepository
                                          .GetAsync(a => a.Id == albumId)
                                          .ConfigureAwait(false);

            var album = albums.FirstOrDefault();

            if (album == null)
            {
                return new Result<AlbumDto>(ResultType.Invalid, "Could not find album!");
            }

            if (loadPerformer)
            {
                album.Performer = (await _unitOfWork.PerformerRepository
                                                    .GetAsync(p => p.Id == album.PerformerId)
                                                    .ConfigureAwait(false))
                                                    .FirstOrDefault();
            }

            return new Result<AlbumDto>(Auto.Mapper.Map<AlbumDto>(album));
        }

        public async Task<Result<AlbumDto>> AddAlbumAsync(Album album)
        {
            var result = new AlbumValidator().Validate(album);

            if (!result.IsValid)
            {
                return new Result<AlbumDto>(ResultType.Invalid, string.Join("; ", result.Errors));
            }

            _unitOfWork.AlbumRepository.Add(album);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<AlbumDto>(Auto.Mapper.Map<AlbumDto>(album));
        }

        public async Task<Result<AlbumDto>> RemoveAlbumAsync(int albumId)
        {
            var albums = await _unitOfWork.AlbumRepository
                                          .GetAsync(a => a.Id == albumId)
                                          .ConfigureAwait(false);

            var album = albums.FirstOrDefault();

            if (album == null)
            {
                return new Result<AlbumDto>(ResultType.Invalid, "Could not find album!");
            }

            _unitOfWork.AlbumRepository.Delete(album);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<AlbumDto>(Auto.Mapper.Map<AlbumDto>(album));
        }

        public async Task<Result<AlbumDto>> UpdateAlbumAsync(int albumId, Album newAlbum)
        {
            var result = new AlbumValidator().Validate(newAlbum);

            if (!result.IsValid)
            {
                return new Result<AlbumDto>(ResultType.Invalid, string.Join("; ", result.Errors));
            }

            var albums = await _unitOfWork.AlbumRepository
                                          .GetAsync(a => a.Id == albumId)
                                          .ConfigureAwait(false);

            var album = albums.FirstOrDefault();

            if (album == null)
            {
                return new Result<AlbumDto>(ResultType.Invalid, "Could not find album!");
            }

            album.Id = albumId;
            album.Name = newAlbum.Name;
            album.TotalTime = newAlbum.TotalTime;
            album.ReleaseYear = newAlbum.ReleaseYear;
            album.Rate = newAlbum.Rate;

            _unitOfWork.AlbumRepository.Edit(album);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<AlbumDto>(Auto.Mapper.Map<AlbumDto>(album));
        }

        public async Task<Result<AlbumDto>> UpdateAlbumRateAsync(int albumId, byte? rate)
        {
            var albums = await _unitOfWork.AlbumRepository
                                          .GetAsync(a => a.Id == albumId)
                                          .ConfigureAwait(false);

            var album = albums.FirstOrDefault();

            if (album == null)
            {
                return new Result<AlbumDto>(ResultType.Invalid, "Could not find album!");
            }

            album.Rate = rate;

            _unitOfWork.AlbumRepository.Edit(album);
            _unitOfWork.Save();

            return new Result<AlbumDto>(Auto.Mapper.Map<AlbumDto>(album));
        }

        public async Task<Result<AlbumDto>> MoveAlbumToPerformerAsync(int albumId, int performerId)
        {
            var albums = await _unitOfWork.AlbumRepository
                                          .GetAsync(a => a.Id == albumId)
                                          .ConfigureAwait(false);

            var album = albums.FirstOrDefault();

            if (album == null)
            {
                return new Result<AlbumDto>(ResultType.Invalid, "Could not find album!");
            }

            album.PerformerId = performerId;

            _unitOfWork.AlbumRepository.Edit(album);
            _unitOfWork.Save();

            return new Result<AlbumDto>(Auto.Mapper.Map<AlbumDto>(album));
        }

        /// <summary>
        /// This version loads all album songs including Album and Album.Performer fields
        /// </summary>
        public async Task<IEnumerable<SongDto>> GetAlbumSongsAsync(int albumId)
        {
            var albums = await _unitOfWork.AlbumRepository
                                          .GetAsync(a => a.Id == albumId)
                                          .ConfigureAwait(false);

            var album = albums.FirstOrDefault();

            if (album == null)
            {
                return null;// new Result<AlbumDto>(ResultType.Invalid, "Could not find album!");
            }

            album.Performer = (await _unitOfWork.PerformerRepository
                                                .GetAsync(p => p.Id == album.PerformerId)
                                                .ConfigureAwait(false))
                                                .FirstOrDefault();

            var songs = await _unitOfWork.SongRepository
                                         .GetAsync(s => s.AlbumId == album.Id)
                                         .ConfigureAwait(false);

            return Auto.Mapper.Map<IEnumerable<SongDto>>(songs);
        }
    }
}
