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

        public override async Task AddAsync(Album album)
        {
            // manual autoincrement
            var lastId = await Context.Albums
                                      .Select(a => a.Id)
                                      .DefaultIfEmpty(0)
                                      .MaxAsync()
                                      .ConfigureAwait(false);
            album.Id = ++lastId;
            Add(album);
        }

        /// <summary>
        /// Load songs of the album lazily
        /// </summary>
        public async Task<IEnumerable<Song>> GetAlbumSongsAsync(Album album)
        {
            var songs = await Context.Songs
                                     .Where(s => s.AlbumId == album.Id)
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
