using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Entities;

namespace MusCat.Repositories
{
    /// <summary>
    /// This repository class slightly differs from ordinary CRUD-repositories:
    /// it's just a read-only repository for stats, 
    /// so we just create db context ad-hoc in each method
    /// </summary>
    class StatsRepository : IStatsRepository
    {
        public int PerformerCount { get; }
        public int AlbumCount { get; }
        public int SongCount { get; }

        public StatsRepository()
        {
            using (var context = new MusCatEntities())
            {
                PerformerCount = context.Performers.Count();
                AlbumCount = context.Albums.Count();
                SongCount = context.Songs.Count();
            }
        }
        
        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int latestCount = 7)
        {
            using (var context = new MusCatEntities())
            {
                return await
                    context.Albums.Include("Performer")
                           .OrderBy(a => a.ID)
                           .Skip(AlbumCount - latestCount)
                           .Take(latestCount)
                           .ToListAsync()
                           .ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<IGrouping<string, Performer>>> GetPerformerCountriesAsync()
        {
            using (var context = new MusCatEntities())
            {
                return await
                    context.Performers
                           .GroupBy(p => p.Country.Name)
                           .Where(g => g.Key != null && g.Count() >= 10)
                           .ToListAsync()
                           .ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<DecadeAlbumsStats>> GetAlbumDecadesAsync()
        {
            using (var context = new MusCatEntities())
            {
                return await
                    context.Albums
                           .GroupBy(a => a.ReleaseYear/10)
                           .Select(g => new DecadeAlbumsStats
                           {
                               Decade = g.Key + "0s",
                               TotalCount = g.Count(),
                               MaxRatedCount = g.Count(x => x.Rate == 10)
                           })
                           .OrderBy(d => d.Decade)
                           .ToListAsync()
                           .ConfigureAwait(false);
            }
        }
    }
}
