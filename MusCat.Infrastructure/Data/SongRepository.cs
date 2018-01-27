using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Infrastructure.Data
{
    class SongRepository : Repository<Song>
    {
        public SongRepository(MusCatDbContext context) : base(context)
        {
        }

        public override async Task AddAsync(Song entity)
        {
            // manual autoincrement
            var lastId = await Context.Songs
                                      .Select(s => s.Id)
                                      .DefaultIfEmpty(0)
                                      .MaxAsync()
                                      .ConfigureAwait(false);
            entity.Id = ++lastId;
            Add(entity);
        }
    }
}
