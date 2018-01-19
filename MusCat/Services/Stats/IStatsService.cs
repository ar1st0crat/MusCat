using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Entities;

namespace MusCat.Services.Stats
{
    interface IStatsService
    {
        Task<int> PerformerCountAsync();
        Task<int> AlbumCountAsync();
        Task<int> SongCountAsync();

        Task<IEnumerable<Album>> GetLatestAlbumsAsync(int latestCount);
        Task<IEnumerable<IGrouping<string, Performer>>> GetPerformerCountriesAsync();
        Task<IEnumerable<DecadeAlbumsStats>> GetAlbumDecadesAsync();
    }
}
