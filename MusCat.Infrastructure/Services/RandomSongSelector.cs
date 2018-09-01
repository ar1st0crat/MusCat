using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Infrastructure.Data;

namespace MusCat.Infrastructure.Services
{
    public class RandomSongSelector : ISongSelector
    {
        private readonly string _connectionString;

        /// <summary>
        /// Randomizer
        /// </summary>
        private readonly Random _songSelector = new Random();
        
        public RandomSongSelector(string connectionString)
        {
            _connectionString = connectionString;
        }

        public Song SelectSong()
        {
            using (var context = new MusCatDbContext(_connectionString))
            {
                // find out the maximum song ID in the database
                var maxSid = context.Songs.Max(s => s.Id);

                var songId = _songSelector.Next() % maxSid;
                var song = context.Songs.First(s => s.Id >= songId);

                // include the corresponding album of our song
                song.Album = context.Albums.First(a => a.Id == song.AlbumId);

                // do the same thing with performer for included album
                song.Album.Performer = context.Performers.First(p => p.Id == song.Album.PerformerId);

                return song;
            }
        }

        public async Task<Song> SelectSongAsync()
        {
            using (var context = new MusCatDbContext(_connectionString))
            {
                // find out the maximum song ID in the database
                var maxSid = context.Songs.Max(s => s.Id);

                var songId = _songSelector.Next() % maxSid;

                var song = await context.Songs
                                        .FirstAsync(s => s.Id >= songId)
                                        .ConfigureAwait(false);
                
                // include the corresponding album of our song
                song.Album = await context.Albums
                                          .FirstAsync(a => a.Id == song.AlbumId)
                                          .ConfigureAwait(false);
                
                // do the same thing with performer for included album
                song.Album.Performer = await context.Performers
                                                    .FirstAsync(p => p.Id == song.Album.PerformerId)
                                                    .ConfigureAwait(false);
                return song;
            }
        }
    }
}
