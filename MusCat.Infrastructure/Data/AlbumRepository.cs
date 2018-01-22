using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;

namespace MusCat.Infrastructure.Data
{
    internal class AlbumRepository : Repository<Album>, IAlbumRepository
    {
        public AlbumRepository(MusCatDbContext context) : base(context)
        {
        }

        public override async Task AddAsync(Album entity)
        {
            // manual autoincrement
            var lastId = await Context.Albums
                                      .Select(a => a.ID)
                                      .DefaultIfEmpty(0)
                                      .MaxAsync()
                                      .ConfigureAwait(false);
            entity.ID = ++lastId;
            Add(entity);
        }

        /// <summary>
        /// Load songs of the album lazily
        /// </summary>
        public async Task<IEnumerable<Song>> GetAlbumSongsAsync(Album album)
        {
            var songs = await Context.Songs
                                     .Where(s => s.AlbumID == album.ID)
                                     .ToListAsync()
                                     .ConfigureAwait(false);
            foreach (var song in songs)
            {
                song.Album = album;
            }

            return songs;
        }
    }
}
