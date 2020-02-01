using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Util;
using MusCat.Application.Dto;
using MusCat.Application.Interfaces;
using MusCat.Application.Validators;

namespace MusCat.Application.Services
{
    public class PerformerService : IPerformerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PerformerService(IUnitOfWork unitOfWork)
        {
            Guard.AgainstNull(unitOfWork);
            _unitOfWork = unitOfWork;
        }

        public async Task<PageCollection<PerformerDto>> GetAllPerformersAsync(int pageIndex = 0, int pageSize = 10)
        {
            var performers = await _unitOfWork.PerformerRepository
                                              .GetPaginatedAsync(pageIndex, pageSize)
                                              .ConfigureAwait(false);

            return Auto.Mapper.Map<PageCollection<PerformerDto>>(performers);
        }


        public async Task<PageCollection<PerformerDto>>
            GetPerformersByFirstLetterAsync(string letter, int pageIndex = 0, int pageSize = 10)
        {
            var performers = await _unitOfWork.PerformerRepository
                                              .GetByFirstLetterAsync(letter, pageIndex, pageSize)
                                              .ConfigureAwait(false);

            return Auto.Mapper.Map<PageCollection<PerformerDto>>(performers);
        }


        public async Task<PageCollection<PerformerDto>>
            GetPerformersBySubstringAsync(string substring, int pageIndex = 0, int pageSize = 10)
        {
            var performers = await _unitOfWork.PerformerRepository
                                              .GetBySubstringAsync(substring, pageIndex, pageSize)
                                              .ConfigureAwait(false);

            return Auto.Mapper.Map<PageCollection<PerformerDto>>(performers);
        }


        public async Task<Result<PerformerDto>> GetPerformerAsync(int performerId)
        {
            var performers = await _unitOfWork.PerformerRepository
                                              .GetAsync(p => p.Id == performerId)
                                              .ConfigureAwait(false);

            var performer = performers.FirstOrDefault();

            if (performer == null)
            {
                return new Result<PerformerDto>(ResultType.Invalid, "Could not find performer!");
            }

            return new Result<PerformerDto>(Auto.Mapper.Map<PerformerDto>(performer));
        }

        
        public async Task<Result<PerformerDto>> AddPerformerAsync(Performer performer)
        {
            var result = new PerformerValidator().Validate(performer);

            if (!result.IsValid)
            {
                return new Result<PerformerDto>(ResultType.Invalid, string.Join("; ", result.Errors));
            }

            _unitOfWork.PerformerRepository.Add(performer);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<PerformerDto>(Auto.Mapper.Map<PerformerDto>(performer));
        }

        
        public async Task<Result<PerformerDto>> RemovePerformerAsync(int performerId)
        {
            var performers = await _unitOfWork.PerformerRepository
                                              .GetAsync(p => p.Id == performerId)
                                              .ConfigureAwait(false);

            var performer = performers.FirstOrDefault();

            if (performer == null)
            {
                return new Result<PerformerDto>(ResultType.Invalid, "Could not find performer!");
            }

            _unitOfWork.PerformerRepository.Delete(performer);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<PerformerDto>(Auto.Mapper.Map<PerformerDto>(performer));
        }


        public async Task<Result<PerformerDto>> UpdatePerformerAsync(int performerId, Performer newPerformer)
        {
            var result = new PerformerValidator().Validate(newPerformer);

            if (!result.IsValid)
            {
                return new Result<PerformerDto>(ResultType.Invalid, string.Join("; ", result.Errors));
            }

            var performers = await _unitOfWork.PerformerRepository
                                              .GetAsync(p => p.Id == performerId)
                                              .ConfigureAwait(false);

            var performer = performers.FirstOrDefault();

            if (performer == null)
            {
                return new Result<PerformerDto>(ResultType.Invalid, "Could not find performer!");
            }

            performer.Id = performerId;
            performer.Name = newPerformer.Name;
            performer.Info = newPerformer.Info;
            performer.CountryId = newPerformer.CountryId;

            _unitOfWork.PerformerRepository.Edit(performer);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<PerformerDto>(Auto.Mapper.Map<PerformerDto>(performer));
        }


        public async Task<Result<AlbumDto>> AddAlbumAsync(int performerId, Album newAlbum)
        {
            var album = new Album
            {
                Name = newAlbum.Name,
                TotalTime = newAlbum.TotalTime,
                PerformerId = performerId,
                ReleaseYear = newAlbum.ReleaseYear,
                Info = newAlbum.Info,
                Rate = newAlbum.Rate
            };

            _unitOfWork.AlbumRepository.Add(album);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<AlbumDto>(Auto.Mapper.Map<AlbumDto>(album));
        }


        public async Task<IEnumerable<AlbumDto>>
            GetPerformerAlbumsAsync(int performerId, string albumPattern = null)
        {
            Expression<Func<Album, bool>> filterExpression;

            if (albumPattern != null)
            {
                filterExpression = a => a.PerformerId == performerId && 
                                        a.Name.ToLower().Contains(albumPattern.ToLower());
            }
            else
            {
                filterExpression = a => a.PerformerId == performerId;
            }

            var albums = await _unitOfWork.AlbumRepository
                                          .GetAsync(a => a.PerformerId == performerId)
                                          .ConfigureAwait(false);

            return Auto.Mapper.Map<IEnumerable<AlbumDto>>(
                    albums.OrderBy(a => a.ReleaseYear).ThenBy(a => a.Name));
        }


        public async Task<int> SongCountAsync(int performerId)
        {
            return await _unitOfWork.SongRepository
                                    .CountAsync(s => s.Album.PerformerId == performerId)
                                    .ConfigureAwait(false);
        }
    }
}
