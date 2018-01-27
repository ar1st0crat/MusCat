using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;

namespace MusCat.Core.Services
{
    public class PerformerService
    {
        private readonly IUnitOfWork _unitOfWork;

        public PerformerService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<Result<Performer>> AddPerformerAsync(string name)
        {
            var performer = new Performer { Name = name };

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
            var performer = _unitOfWork.PerformerRepository
                                     .Get(c => c.Id == performerId)
                                     .FirstOrDefault();
            if (performer == null)
            {
                return new Result<Performer>(ResultType.Invalid, "Could not find Performer!");
            }

            _unitOfWork.PerformerRepository.Delete(performer);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Performer>(performer);
        }

        public async Task<Result<Performer>> UpdatePerformerAsync(long performerId, string name, string info, byte? countryId)
        {
            var performer = new Performer { Name = name, Info = info, CountryId = countryId };

            if (performer.Error != string.Empty)
            {
                return new Result<Performer>(ResultType.Invalid, performer.Error);
            }

            performer = _unitOfWork.PerformerRepository
                                   .Get(c => c.Id == performerId)
                                   .FirstOrDefault();

            if (performer == null)
            {
                return new Result<Performer>(ResultType.Invalid, "Could not find Performer!");
            }

            performer.Name = name;
            performer.Info = info;
            performer.CountryId = countryId;

            _unitOfWork.PerformerRepository.Edit(performer);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Performer>(performer);
        }

        public async Task<Result<Album>> AddAlbumAsync(long performerId, string name, short year, string duration)
        {
            var album = new Album
            {
                Name = name,
                TotalTime = duration,
                PerformerId = performerId,
                ReleaseYear = year
            };

            await _unitOfWork.AlbumRepository.AddAsync(album).ConfigureAwait(false);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<Album>(album);
        }
    }
}
