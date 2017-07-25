using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Model;

namespace MusCat.Repository
{
    class DecadeAlbumsStats
    {
        public string Decade { get; set; }
        public int TotalCount { get; set; }
        public int MaxRatedCount { get; set; }
    }

    interface IStatsRepository
    {
        Task<IEnumerable<Album>> GetLatestAlbumsAsync(int latestCount);
        Task<IEnumerable<IGrouping<string, Performer>>> GetPerformerCountriesAsync();
        Task<IEnumerable<DecadeAlbumsStats>> GetAlbumDecadesAsync();
        int Count<T>();
    }
}
