using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Domain
{
    public interface IPerformerService
    {
        Task<Result<Performer>> AddPerformerAsync(string name);
        Task<Result<Performer>> RemovePerformerAsync(long performerId);
        Task<Result<Performer>> UpdatePerformerAsync(long performerId, string name, string info, byte? countryId);
        Task<Result<Album>> AddAlbumAsync(long performerId, string name, short year, string duration);
        Task<Result<Country>> GetCountryAsync(long performerId);
    }
}