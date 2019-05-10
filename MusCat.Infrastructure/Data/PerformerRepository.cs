using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;

namespace MusCat.Infrastructure.Data
{
    class PerformerRepository : Repository<Performer>, IPerformerRepository
    {
        public PerformerRepository(MusCatDbContext context) : base(context)
        {
        }

        /// <summary>
        /// Query can be of two kinds:
        /// 
        /// 1) single letters ('a', 'b', 'c', ..., 'z')
        /// 2) the "Other" option (all performers whose name doesn't start with capital English letter, e.g. "10CC", "Пикник", etc.)
        /// 
        /// </summary>
        /// <param name="letter"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PageCollection<Performer>> 
            GetByFirstLetterAsync(string letter, int pageIndex = 0, int pageSize = 10)
        {
            Expression<Func<Performer, bool>> filterExpression;

            if (letter.Length == 1)
            {
                filterExpression = p => p.Name.ToLower().StartsWith(letter);
            }
            else
            {
                filterExpression = p => string.Compare(p.Name.ToLower().Substring(0, 1), "a", StringComparison.Ordinal) < 0 ||
                                        string.Compare(p.Name.ToLower().Substring(0, 1), "z", StringComparison.Ordinal) > 0;
            }

            return await GetPaginatedAsync(filterExpression, pageIndex, pageSize).ConfigureAwait(false);
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="substring"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PageCollection<Performer>> 
            GetBySubstringAsync(string substring, int pageIndex = 0, int pageSize = 10)
        {
            Expression<Func<Performer, bool>> filterExpression = 
                p => p.Name.ToLower().Contains(substring.ToLower());

            return await GetPaginatedAsync(filterExpression, pageIndex, pageSize).ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="substring"></param>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<PageCollection<Performer>>
            GetByAlbumSubstringAsync(string substring, int pageIndex = 0, int pageSize = 10)
        {
            Expression<Func<Performer, bool>> filterExpression =
                p => p.Albums.Any(a => a.Name.Contains(substring));

            return await GetPaginatedAsync(filterExpression, pageIndex, pageSize).ConfigureAwait(false);
        }

        public async Task<PageCollection<Performer>> GetPaginatedAsync(int pageIndex, int pageSize)
        {
            return new PageCollection<Performer>
            {
                Items = await Context.Performers.Include("Country")
                                     .OrderBy(p => p.Name)
                                     .Skip(pageIndex * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync()
                                     .ConfigureAwait(false),
                ItemsPerPage = pageSize,
                TotalItems = await Context.Performers.CountAsync().ConfigureAwait(false)
            };
        }

        public async Task<PageCollection<Performer>>
            GetPaginatedAsync(Expression<Func<Performer, bool>> filterExpression, int pageIndex, int pageSize)
        {
            return new PageCollection<Performer>
            {
                Items = await Context.Performers.Include("Country")
                                     .Where(filterExpression)
                                     .OrderBy(p => p.Name)
                                     .Skip(pageIndex * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync()
                                     .ConfigureAwait(false),
                ItemsPerPage = pageSize,
                TotalItems = await CountAsync(filterExpression).ConfigureAwait(false)
            };
        }
    }
}
