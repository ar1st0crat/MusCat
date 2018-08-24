using System.Data.Entity;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Infrastructure.Data
{
    /// <summary>
    /// The class for manual auto-incremention of different Entities.
    /// It provides explicit specializations of Repository.AddAsync() generic method.
    /// 
    /// Yes, this approach looks quite ugly.
    /// 
    /// Historically there were no auto-increment keys in MusCat database.
    /// In previous versions of MusCat app the AddAsync() method was polymorphic
    /// and the corresponding auto-incrementing logic was implemented in subclasses.
    /// 
    /// However, after refactorings those Repository subclasses became empty.
    /// 
    /// </summary>
    /// <typeparam name="T">Entity type</typeparam>
    static class ManualAutoIncrement<T>
    {
        public static async Task DoAsync(MusCatDbContext context, T entity)
        {
            if (entity.GetType() == typeof(Performer))
            {
                var performer = entity as Performer;

                var lastId = await context.Performers
                                          .MaxAsync(p => p.Id)
                                          .ConfigureAwait(false);
                performer.Id = ++lastId;
                context.Performers.Add(performer);
            }

            else if (entity.GetType() == typeof(Album))
            {
                var album = entity as Album;

                var lastId = await context.Albums
                                          .MaxAsync(a => a.Id)
                                          .ConfigureAwait(false);
                album.Id = ++lastId;
                context.Albums.Add(album);
            }

            else if (entity.GetType() == typeof(Song))
            {
                var song = entity as Song;

                var lastId = await context.Songs
                                          .MaxAsync(s => s.Id)
                                          .ConfigureAwait(false);
                song.Id = ++lastId;
                context.Songs.Add(song);
            }

            else if (entity.GetType() == typeof(Country))
            {
                var country = entity as Country;

                var lastId = await context.Countries
                                          .MaxAsync(c => c.Id)
                                          .ConfigureAwait(false);
                country.Id = ++lastId;
                context.Countries.Add(country);
            }
        }
    }
}
