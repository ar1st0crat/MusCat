using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Stats;
using MusCat.Infrastructure.Data;

namespace MusCat.Infrastructure.Services.Stats
{
    /// <summary>
    /// This particular version of a stats service class could be an ordinary repository
    /// however, in general, statistical data are not tied to databases
    /// so the class is not inherited from Repository.
    /// </summary>
    public class StatsService : IStatsService
    {
        private readonly string _connectionString;

        public StatsService(string conn)
        {
            _connectionString = conn;
        }

        public async Task<int> PerformerCountAsync()
        {
            using (var context = new MusCatDbContext(_connectionString))
            {
                return await context.Performers.CountAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> AlbumCountAsync()
        {
            using (var context = new MusCatDbContext(_connectionString))
            {
                return await context.Albums.CountAsync().ConfigureAwait(false);
            }
        }

        public async Task<int> SongCountAsync()
        {
            using (var context = new MusCatDbContext(_connectionString))
            {
                return await context.Songs.CountAsync().ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int latestCount = 7)
        {
            using (var context = new MusCatDbContext(_connectionString))
            {
                return await
                    context.Albums.Include("Performer").AsNoTracking()
                                  .OrderByDescending(a => a.Id)
                                  .Take(latestCount)
                                  .ToListAsync()
                                  .ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<Performer>> GetTopPerformersAsync(int count = 7, string country = null)
        {
            using (var context = new MusCatDbContext(_connectionString))
            {
                return await
                    context.Performers.Include("Country").AsNoTracking()
                                      .Where(p => p.Country.Name == country)
                                      .OrderByDescending(p => p.Albums.Sum(a => a.Rate))
                                      .Take(count)
                                      .ToListAsync()
                                      .ConfigureAwait(false);
            }
        }

        public async Task<IDictionary<string, int>> GetPerformerCountriesAsync()
        {
            using (var context = new MusCatDbContext(_connectionString))
            {
                return await
                    context.Performers.AsNoTracking()
                           .GroupBy(p => p.Country.Name)
                           .Where(g => g.Key != null)
                           .ToDictionaryAsync(g => g.Key, g => g.Count())
                           .ConfigureAwait(false);
            }
        }

        public async Task<IEnumerable<DecadeAlbumsStats>> GetAlbumDecadesAsync()
        {
            using (var context = new MusCatDbContext(_connectionString))
            {
                return await
                    context.Albums.AsNoTracking()
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
        }
    }
}
