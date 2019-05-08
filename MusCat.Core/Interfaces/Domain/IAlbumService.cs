using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Domain
{
    public interface IAlbumService
    {
        Task<Result<Album>> AddAlbumAsync(Album album);
        Task<Result<Album>> RemoveAlbumAsync(int albumId);
        Task<Result<Album>> UpdateAlbumAsync(Album album);
        Task<Result<Album>> UpdateAlbumRateAsync(int albumId, byte? rate);
        Task<Result<Album>> MoveAlbumToPerformerAsync(int albumId, int performerId);
        Task<IEnumerable<Song>> LoadAlbumSongsAsync(int albumId);
    }
}