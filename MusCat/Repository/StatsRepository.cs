using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Model;
using MusCat.Repository.Base;

namespace MusCat.Repository
{
    class StatsRepository : DisposableRepository<MusCatEntities>, IStatsRepository
    {
        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int latestCount = 7)
        {
            var totalCount = await Context.Albums.CountAsync();

            return await
                Context.Albums.Include("Performer")
                       .OrderBy(a => a.ID)
                       .Skip(totalCount - latestCount)
                       .Take(latestCount)
                       .ToListAsync()
                       .ConfigureAwait(false);
        }

        public async Task<IEnumerable<IGrouping<string, Performer>>> GetPerformerCountriesAsync()
        {
            return await
                Context.Performers
                       .GroupBy(p => p.Country.Name)
                       .Where(g => g.Key != null && g.Count() >= 10)
                       .ToListAsync()
                       .ConfigureAwait(false);
        }

        public async Task<IEnumerable<DecadeAlbumsStats>> GetAlbumDecadesAsync()
        {
            return await
                Context.Albums
                       .GroupBy(a => a.ReleaseYear / 10)
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

        public int Count<T>()
        {
            if (typeof(Song).IsAssignableFrom(typeof(T)))
            {
                return Context.Songs.Count();
            }

            if (typeof(Album).IsAssignableFrom(typeof(T)))
            {
                return Context.Albums.Count();
            }

            return Context.Performers.Count();
        }
    }
}
