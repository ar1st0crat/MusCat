using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Data
{
    public interface IAlbumRepository : IRepository<Album>
    {
        Task<IEnumerable<Song>> GetAlbumSongsAsync(Album album);
    }
}
