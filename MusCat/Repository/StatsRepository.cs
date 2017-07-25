using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Model;

namespace MusCat.Repository
{
    class StatsRepository : IDisposable
    {
        public MusCatEntities Context { get; set; } = new MusCatEntities();

        public async Task<IEnumerable<Album>> GetLatestAlbumsAsync(int latestCount = 7)
        {
            var totalCount = await Context.Albums.CountAsync();

            return await Context.Albums.Include("Performer")
                                .OrderBy(a => a.ID)
                                .Skip(totalCount - latestCount - 167)
                                .Take(latestCount)
                                .ToListAsync()
                                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<IGrouping<string, Performer>>> GetPerformerCountriesAsync()
        {
            return await Context.Performers
                                .GroupBy(p => p.Country.Name)
                                .Where(g => g.Key != null && g.Count() >= 10)
                                .ToListAsync()
                                .ConfigureAwait(false);
        }

        public async Task<IEnumerable<AlbumDecade>> GetAlbumDecadesAsync()
        {
            return await Context.Albums
                                .GroupBy(a => a.ReleaseYear / 10)
                                .Select(g => new AlbumDecade
                                {
                                    Decade = g.Key + "0s",
                                    TotalCount = g.Count(),
                                    MaxRatedCount = g.Count(x => x.Rate == 10)
                                })
                                .OrderBy(d => d.Decade)
                                .ToListAsync()
                                .ConfigureAwait(false);
        }

        public int PerformerCount => Context.Performers.Count();
        public int AlbumCount => Context.Albums.Count();
        public int SongCount => Context.Songs.Count();

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        internal class AlbumDecade
        {
            public string Decade { get; set; }
            public int TotalCount { get; set; }
            public int MaxRatedCount { get; set; }
        }
    }
}
