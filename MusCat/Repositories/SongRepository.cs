using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Entities;
using MusCat.Repositories.Base;

namespace MusCat.Repositories
{
    class SongRepository : Repository<Song>
    {
        public SongRepository(MusCatEntities context) : base(context)
        {
        }

        public override async Task AddAsync(Song entity)
        {
            // manual autoincrement
            var lastId = await Context.Songs
                                      .Select(s => s.ID)
                                      .DefaultIfEmpty(0)
                                      .MaxAsync()
                                      .ConfigureAwait(false);
            entity.ID = ++lastId;
            Add(entity);
        }
    }
}
