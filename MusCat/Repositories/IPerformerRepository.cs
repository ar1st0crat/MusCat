using System.Threading.Tasks;
using MusCat.Entities;
using MusCat.Repositories.Base;

namespace MusCat.Repositories
{
    interface IPerformerRepository : IRepository<Performer>
    {
        Task<PageCollection<Performer>> GetByFirstLetterAsync(string letter, int pageIndex, int pageSize);
        Task<PageCollection<Performer>> GetBySubstringAsync(string substring, int pageIndex, int pageSize);
        Task<PageCollection<Performer>> GetByAlbumSubstringAsync(string substring, int pageIndex, int pageSize);
        Task<int> CountByFirstLetterAsync(string letter);
        Task<int> CountBySubstringAsync(string letter);
        Task<int> CountByAlbumSubstringAsync(string letter);
    }
}
