using System.Threading.Tasks;
using MusCat.Application.Dto;
using MusCat.Core.Entities;

namespace MusCat.Application.Interfaces
{
    public interface ISongService
    {
        Task<Result<SongDto>> AddSongAsync(Song Song);
        Task<Result<SongDto>> RemoveSongAsync(int SongId);
        Task<Result<SongDto>> UpdateSongAsync(Song newSong);
    }
}