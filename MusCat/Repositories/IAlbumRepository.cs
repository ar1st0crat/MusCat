using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Entities;
using MusCat.Repositories.Base;

namespace MusCat.Repositories
{
    interface IAlbumRepository : IRepository<Album>
    {
        Task<IEnumerable<Song>> GetAlbumSongsAsync(Album album);
    }
}
