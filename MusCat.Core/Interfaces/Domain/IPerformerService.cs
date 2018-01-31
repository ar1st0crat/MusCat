using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Domain
{
    public interface IPerformerService
    {
        Task<Result<Performer>> AddPerformerAsync(Performer performer);
        Task<Result<Performer>> RemovePerformerAsync(long performerId);
        Task<Result<Performer>> UpdatePerformerAsync(Performer performer);
        Task<Result<Album>> AddAlbumAsync(long performerId, Album album);
        Task<Result<Country>> GetCountryAsync(long performerId);
    }
}