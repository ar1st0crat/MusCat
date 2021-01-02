using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Application.Dto;
using MusCat.Core.Entities;

namespace MusCat.Application.Interfaces
{
    public interface IAlbumService
    {
        Task<IEnumerable<AlbumDto>> GetAllAlbumsAsync();
        Task<Result<AlbumDto>> GetAlbumAsync(int albumId, bool loadPerformer = false);
        Task<Result<AlbumDto>> AddAlbumAsync(Album album);
        Task<Result<AlbumDto>> RemoveAlbumAsync(int albumId);
        Task<Result<AlbumDto>> UpdateAlbumAsync(int albumId, Album album);
        Task<Result<AlbumDto>> UpdateAlbumRateAsync(int albumId, byte? rate);
        Task<Result<SongDto>> UpdateSongRateAsync(int songId, byte? rate);
        Task<Result<AlbumDto>> MoveAlbumToPerformerAsync(int albumId, int performerId);
        Task<IEnumerable<SongDto>> GetAlbumSongsAsync(int albumId);
    }
}