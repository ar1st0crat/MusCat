using System.Collections.Generic;
using System.Threading.Tasks;
using MusCat.Model;
using MusCat.Repository.Base;

namespace MusCat.Repository
{
    interface IPerformerRepository : IRepository<Performer>
    {
        Task<IEnumerable<Performer>> GetByFirstLetterAsync(string letter, int pageIndex, int pageSize);
        Task<IEnumerable<Performer>> GetBySubstringAsync(string substring, int pageIndex, int pageSize);
        Task<IEnumerable<Performer>> GetByAlbumSubstringAsync(string substring, int pageIndex, int pageSize);
        Task<int> CountByFirstLetterAsync(string letter);
        Task<int> CountBySubstringAsync(string letter);
        Task<int> CountByAlbumSubstringAsync(string letter);
    }
}
