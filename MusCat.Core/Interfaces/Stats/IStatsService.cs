using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Stats
{
    public interface IStatsService
    {
        Task<int> PerformerCountAsync();
        Task<int> AlbumCountAsync();
        Task<int> SongCountAsync();

        Task<IEnumerable<Album>> GetLatestAlbumsAsync(int latestCount);
        Task<IEnumerable<IGrouping<string, Performer>>> GetPerformerCountriesAsync();
        Task<IEnumerable<DecadeAlbumsStats>> GetAlbumDecadesAsync();
    }
}
