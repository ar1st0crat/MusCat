using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Data
{
    public interface IPerformerRepository : IRepository<Performer>
    {
        Task<PageCollection<Performer>> GetByFirstLetterAsync(string letter, int pageIndex, int pageSize);
        Task<PageCollection<Performer>> GetBySubstringAsync(string substring, int pageIndex, int pageSize);
        Task<PageCollection<Performer>> GetByAlbumSubstringAsync(string substring, int pageIndex, int pageSize);
        Task<IEnumerable<Album>> GetPerformerAlbumsAsync(int performerId, string albumPattern = null);
        Task<int> CountByFirstLetterAsync(string letter);
        Task<int> CountBySubstringAsync(string substring);
        Task<int> CountByAlbumSubstringAsync(string substring);
    }
}
