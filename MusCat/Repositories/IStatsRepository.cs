using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Entities;

namespace MusCat.Repositories
{
    class DecadeAlbumsStats
    {
        public string Decade { get; set; }
        public int TotalCount { get; set; }
        public int MaxRatedCount { get; set; }
    }

    interface IStatsRepository
    {
        int PerformerCount { get; }
        int AlbumCount { get; }
        int SongCount { get; }

        Task<IEnumerable<Album>> GetLatestAlbumsAsync(int latestCount);
        Task<IEnumerable<IGrouping<string, Performer>>> GetPerformerCountriesAsync();
        Task<IEnumerable<DecadeAlbumsStats>> GetAlbumDecadesAsync();
    }
}
