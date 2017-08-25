using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Entities;
using MusCat.Repositories.Base;

namespace MusCat.Repositories
{
    class PerformerRepository : 
        Repository<MusCatEntities, Performer>, 
        IPerformerRepository
    {
        public async Task<IEnumerable<Performer>> 
            GetByFirstLetterAsync(string letter, int pageIndex = 0, int pageSize = 10)
        {
            // query can be of two kinds:

            // 1) single letters ('a', 'b', 'c', ..., 'z')
            if (letter.Length == 1)
            {
                return await
                    Context.Performers.Include("Country").Include("Albums")
                           .Where(p => p.Name.ToLower().StartsWith(letter))
                           .OrderBy(p => p.Name)
                           .Skip(pageIndex * pageSize)
                           .Take(pageSize)
                           .ToListAsync()
                           .ConfigureAwait(false);
            }
            // 2) The "Other" option
            //    (all performers whose name doesn't start with capital English letter, e.g. "10CC", "Пикник", etc.)
            else
            {
                return await
                    Context.Performers.Include("Country").Include("Albums")
                           .Where(p => string.Compare(p.Name.ToLower().Substring(0, 1), "a", StringComparison.Ordinal) < 0 ||
                                       string.Compare(p.Name.ToLower().Substring(0, 1), "z", StringComparison.Ordinal) > 0)
                           .OrderBy(p => p.Name)
                           .Skip(pageIndex * pageSize)
                           .Take(pageSize)
                           .ToListAsync()
                           .ConfigureAwait(false);
            }
        }
        
        public async Task<IEnumerable<Performer>> 
            GetBySubstringAsync(string substring, int pageIndex = 0, int pageSize = 10)
        {
            return await
                Context.Performers.Include("Country").Include("Albums")
                       .Where(p => p.Name.ToLower().Contains(substring.ToLower()))
                       .OrderBy(p => p.Name)
                       .Skip(pageIndex * pageSize)
                       .Take(pageSize)
                       .ToListAsync()
                       .ConfigureAwait(false);
        }

        public async Task<IEnumerable<Performer>>
            GetByAlbumSubstringAsync(string substring, int pageIndex = 0, int pageSize = 10)
        {
            return await
                Context.Performers.Include("Country").Include("Albums")
                       .Where(p => p.Albums.Any(a => a.Name.Contains(substring)))
                       .OrderBy(p => p.Name)
                       .Skip(pageIndex * pageSize)
                       .Take(pageSize)
                       .ToListAsync()
                       .ConfigureAwait(false);
        }

        public async Task<int> CountByFirstLetterAsync(string letter)
        {
            if (letter.Length == 1)
            {
                return await
                    Context.Performers
                           .Where(p => p.Name.ToLower().StartsWith(letter))
                           .CountAsync()
                           .ConfigureAwait(false);
            }
            else
            {
                return await
                    Context.Performers
                           .Where(p => string.Compare(p.Name.ToLower().Substring(0, 1), "a", StringComparison.Ordinal) < 0 ||
                                       string.Compare(p.Name.ToLower().Substring(0, 1), "z", StringComparison.Ordinal) > 0)
                           .CountAsync()
                           .ConfigureAwait(false);
            }
        }

        public async Task<int> CountBySubstringAsync(string substring)
        {
            return await
                Context.Performers
                       .Where(p => p.Name.ToLower().Contains(substring.ToLower()))
                       .CountAsync()
                       .ConfigureAwait(false);
        }

        public async Task<int> CountByAlbumSubstringAsync(string substring)
        {
            return await
                Context.Performers
                       .Where(p => p.Albums.Any(a => a.Name.Contains(substring)))
                       .CountAsync()
                       .ConfigureAwait(false);
        }
    }
}
