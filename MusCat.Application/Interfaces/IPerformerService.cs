using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Application.Dto;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;

namespace MusCat.Application.Interfaces
{
    public interface IPerformerService
    {
        Task<PageCollection<PerformerDto>> GetAllPerformersAsync(int pageIndex, int pageSize);
        Task<PageCollection<PerformerDto>> GetPerformersByFirstLetterAsync(string letter, int pageIndex, int pageSize);
        Task<PageCollection<PerformerDto>> GetPerformersBySubstringAsync(string substring, int pageIndex, int pageSize);
        Task<Result<PerformerDto>> GetPerformerAsync(int performerId);
        Task<Result<PerformerDto>> AddPerformerAsync(Performer performer);
        Task<Result<PerformerDto>> RemovePerformerAsync(int performerId);
        Task<Result<PerformerDto>> UpdatePerformerAsync(int performerId, Performer performer);
        Task<IEnumerable<AlbumDto>> GetPerformerAlbumsAsync(int performerId, string albumPattern = null);
        Task<Result<AlbumDto>> AddAlbumAsync(int performerId, Album album);
        Task<int> SongCountAsync(int performerId);
    }
}