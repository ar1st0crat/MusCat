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

            await _unitOfWork.PerformerRepository.AddAsync(performer).ConfigureAwait(false);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Performer>(performer);
        }

        public async Task<Result<Performer>> RemovePerformerAsync(long performerId)
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

        public async Task<Result<Album>> AddAlbumAsync(long performerId, Album newAlbum)
        {
            var album = new Album
            {
                Name = newAlbum.Name,
                TotalTime = newAlbum.TotalTime,
                PerformerId = performerId,
                ReleaseYear = newAlbum.ReleaseYear
            };

            await _unitOfWork.AlbumRepository.AddAsync(album).ConfigureAwait(false);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Album>(album);
        }

        public async Task<Result<Country>> GetCountryAsync(long performerId)
        {
            var performers =
                await _unitOfWork.PerformerRepository
                                 .GetAsync(p => p.Id == performerId)
                                 .ConfigureAwait(false);

            var performer = performers.FirstOrDefault();

            var countries = await _unitOfWork.CountryRepository
                             .GetAsync(c => c.Id == performer.CountryId)
                             .ConfigureAwait(false);
            
            return new Result<Country>(countries.First());
        }
    }
}
