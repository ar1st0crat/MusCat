using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Domain
{
    public interface IAlbumService
    {
        Task<Result<Album>> AddAlbumAsync(string name);
        Task<Result<Album>> RemoveAlbumAsync(long albumId);
        Task<Result<Album>> UpdateAlbumAsync(long albumId, string name, string totalTime, short year);
        Result<Album> UpdateAlbumRate(long albumId, byte? rate);
        Task<IEnumerable<Song>> LoadAlbumSongsAsync(long albumId);
    }
}