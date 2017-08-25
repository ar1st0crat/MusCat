using MusCat.Entities;
using MusCat.Repositories.Base;

namespace MusCat.Repositories
{
    class SongRepository : Repository<MusCatEntities, Song>
    {
        public Song GetRandomSongAsync()
        {
            return null;
        }
    }
}
