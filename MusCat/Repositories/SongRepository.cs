using MusCat.Entities;
using MusCat.Repositories.Base;

namespace MusCat.Repositories
{
    class SongRepository : Repository<Song>
    {
        public SongRepository(MusCatEntities context) : base(context)
        {
        }

        public Song GetRandomSongAsync()
        {
            return null;
        }
    }
}
