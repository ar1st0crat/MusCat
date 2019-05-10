using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Data
{
    public interface IPerformerRepository : IRepository<Performer>
    {
        Task<PageCollection<Performer>> GetPaginatedAsync(int pageIndex, int pageSize);
        Task<PageCollection<Performer>> GetPaginatedAsync(Expression<Func<Performer, bool>> filterExpression, int pageIndex, int pageSize);
        Task<PageCollection<Performer>> GetByFirstLetterAsync(string letter, int pageIndex, int pageSize);
        Task<PageCollection<Performer>> GetBySubstringAsync(string substring, int pageIndex, int pageSize);
        Task<PageCollection<Performer>> GetByAlbumSubstringAsync(string substring, int pageIndex, int pageSize);
    }
}
