using System;
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
    class StatsService : IStatsService, IDisposable
    {
        private readonly MusCatDbContext _context = new MusCatDbContext();

        public async Task<int> PerformerCountAsync()
        {
            return await _context.Performers.CountAsync().ConfigureAwait(false);
        }

        public async Task<int> AlbumCountAsync()
        {
            return await _context.Albums.CountAsync().ConfigureAwait(false);
        }

        public Task<int> SongCountAsync()
        {
            return _context.Songs.CountAsync();
        }

        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int latestCount = 7)
        {
            return await
                   _context.Albums.Include("Performer").AsNoTracking()
                           .OrderByDescending(a => a.ID)
                           .Take(latestCount)
                           .ToListAsync()
                           .ConfigureAwait(false);
        }

        public async Task<IEnumerable<IGrouping<string, Performer>>> GetPerformerCountriesAsync()
        {
            return await
                   _context.Performers.AsNoTracking()
                           .GroupBy(p => p.Country.Name)
                           .Where(g => g.Key != null && g.Count() >= 10)
                           .ToListAsync()
                           .ConfigureAwait(false);
        }

        public async Task<IEnumerable<DecadeAlbumsStats>> GetAlbumDecadesAsync()
        {
            return await
                   _context.Albums.AsNoTracking()
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

        #region Dispose pattern

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                }
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
