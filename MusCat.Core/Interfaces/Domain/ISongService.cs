using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Domain
{
    public interface ISongService
    {
        Task<Result<Song>> AddSongAsync(Song Song);
        Task<Result<Song>> RemoveSongAsync(long SongId);
        Task<Result<Song>> UpdateSongAsync(Song newSong);
    }
}