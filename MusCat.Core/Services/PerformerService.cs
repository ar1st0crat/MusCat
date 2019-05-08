using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Interfaces.Domain;
using MusCat.Core.Util;

namespace MusCat.Core.Services
{
    public class PerformerService : IPerformerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PerformerService(IUnitOfWork unitOfWork)
        {
            Guard.AgainstNull(unitOfWork);
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Result<Performer>> AddPerformerAsync(Performer performer)
        {
            if (performer.Error != string.Empty)
            {
                return new Result<Performer>(ResultType.Invalid, performer.Error);
            }

            _unitOfWork.PerformerRepository.Add(performer);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Performer>(performer);
        }

        public async Task<Result<Performer>> RemovePerformerAsync(int performerId)
        {
            var performers = await _unitOfWork.PerformerRepository
                                              .GetAsync(c => c.Id == performerId)
                                              .ConfigureAwait(false);
                                     
            var performer = performers.FirstOrDefault();

            if (performer == null)
            {
                return new Result<Performer>(ResultType.Invalid, "Could not find Performer!");
            }

            _unitOfWork.PerformerRepository.Delete(performer);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Performer>(performer);
        }

        public async Task<Result<Performer>> UpdatePerformerAsync(Performer newPerformer)
        {
            if (newPerformer.Error != string.Empty)
            {
                return new Result<Performer>(ResultType.Invalid, newPerformer.Error);
            }

            var performer = _unitOfWork.PerformerRepository
                                       .Get(c => c.Id == newPerformer.Id)
                                       .FirstOrDefault();

            if (performer == null)
            {
                return new Result<Performer>(ResultType.Invalid, "Could not find Performer!");
            }

            performer.Name = newPerformer.Name;
            performer.Info = newPerformer.Info;
            performer.CountryId = newPerformer.CountryId;

            _unitOfWork.PerformerRepository.Edit(performer);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Performer>(performer);
        }

        public async Task<Result<Album>> AddAlbumAsync(int performerId, Album newAlbum)
        {
            var album = new Album
            {
                Name = newAlbum.Name,
                TotalTime = newAlbum.TotalTime,
                PerformerId = performerId,
                ReleaseYear = newAlbum.ReleaseYear
            };

            _unitOfWork.AlbumRepository.Add(album);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Album>(album);
        }

        public async Task<Result<Country>> GetCountryAsync(int performerId)
        {
            var performers =
                await _unitOfWork.PerformerRepository
                                 .GetAsync(p => p.Id == performerId)
                                 .ConfigureAwait(false);

            var performer = performers.FirstOrDefault();

            var countries = await _unitOfWork.CountryRepository
                                             .GetAsync(c => c.Id == performer.CountryId)
                                             .ConfigureAwait(false);
            
            return new Result<Country>(countries.FirstOrDefault());
        }

        public async Task<int> SongCountAsync(int performerId)
        {
            return await _unitOfWork.SongRepository
                                    .CountAsync(s => s.Album.PerformerId == performerId)
                                    .ConfigureAwait(false);
        }
    }
}
