using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Domain
{
    public interface IPerformerService
    {
        Task<Result<Performer>> AddPerformerAsync(Performer performer);
        Task<Result<Performer>> RemovePerformerAsync(int performerId);
        Task<Result<Performer>> UpdatePerformerAsync(Performer performer);
        Task<Result<Album>> AddAlbumAsync(int performerId, Album album);
        Task<Result<Country>> GetCountryAsync(int performerId);
        Task<int> SongCountAsync(int performerId);
    }
}