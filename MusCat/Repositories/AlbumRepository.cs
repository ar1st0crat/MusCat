using MusCat.Entities;
using MusCat.Repositories.Base;

namespace MusCat.Repositories
{
    internal class AlbumRepository : Repository<Album>
    {
        public AlbumRepository(MusCatEntities context) : base(context)
        {
        }


    }
}
