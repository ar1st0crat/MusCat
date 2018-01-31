using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Domain
{
    public interface IAlbumService
    {
        Task<Result<Album>> AddAlbumAsync(Album album);
        Task<Result<Album>> RemoveAlbumAsync(long albumId);
        Task<Result<Album>> UpdateAlbumAsync(Album album);
        Result<Album> UpdateAlbumRate(long albumId, byte? rate);
        Task<IEnumerable<Song>> LoadAlbumSongsAsync(long albumId);
    }
}