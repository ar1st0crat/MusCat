using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
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

        public override async Task AddAsync(Performer entity)
        {
            // manual autoincrement
            var lastId = await Context.Performers.AsNoTracking()
                                      .Select(p => p.ID)
                                      .DefaultIfEmpty(0)
                                      .MaxAsync()
                                      .ConfigureAwait(false);
            entity.ID = ++lastId;
            Add(entity);
        }

        public async Task<PageCollection<Performer>> 
            GetByFirstLetterAsync(string letter, int pageIndex = 0, int pageSize = 10)
        {
            List<Performer> performers;

            // query can be of two kinds:

            // 1) single letters ('a', 'b', 'c', ..., 'z')
            if (letter.Length == 1)
            {
                performers = await
                    Context.Performers.Include("Country")
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
                performers = await
                    Context.Performers.Include("Country")
                        .Where(
                            p =>
                                string.Compare(p.Name.ToLower().Substring(0, 1), "a", StringComparison.Ordinal) < 0 ||
                                string.Compare(p.Name.ToLower().Substring(0, 1), "z", StringComparison.Ordinal) > 0)
                        .OrderBy(p => p.Name)
                        .Skip(pageIndex * pageSize)
                        .Take(pageSize)
                        .ToListAsync()
                        .ConfigureAwait(false);
            }

            return new PageCollection<Performer>
            {
                Items = performers,
                ItemsPerPage = pageSize,
                TotalItems = await CountByFirstLetterAsync(letter).ConfigureAwait(false)
            };
        }
        
        public async Task<PageCollection<Performer>> 
            GetBySubstringAsync(string substring, int pageIndex = 0, int pageSize = 10)
        {
            return new PageCollection<Performer>
            {
                Items = await Context.Performers.Include("Country")
                                     .Where(p => p.Name.ToLower().Contains(substring.ToLower()))
                                     .OrderBy(p => p.Name)
                                     .Skip(pageIndex * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync()
                                     .ConfigureAwait(false),
                ItemsPerPage = pageSize,
                TotalItems = await CountBySubstringAsync(substring).ConfigureAwait(false)
            };
        }

        public async Task<PageCollection<Performer>>
            GetByAlbumSubstringAsync(string substring, int pageIndex = 0, int pageSize = 10)
        {
            return new PageCollection<Performer>
            {
                Items = await Context.Performers.Include("Country")
                                     .Where(p => p.Albums.Any(a => a.Name.Contains(substring)))
                                     .OrderBy(p => p.Name)
                                     .Skip(pageIndex * pageSize)
                                     .Take(pageSize)
                                     .ToListAsync()
                                     .ConfigureAwait(false),
                ItemsPerPage = pageSize,
                TotalItems = await CountByAlbumSubstringAsync(substring).ConfigureAwait(false)
            };
        }

        public async Task<IEnumerable<Album>>
            GetPerformerAlbumsAsync(Performer performer, string albumPattern = null)
        {
            if (albumPattern != null)
            {
                return await Context.Albums
                                    .Where(a => a.PerformerID == performer.ID)
                                    .Where(a => a.Name.ToLower().Contains(albumPattern.ToLower()))
                                    .OrderBy(a => a.ReleaseYear)
                                    .ThenBy(a => a.Name)
                                    .ToListAsync()
                                    .ConfigureAwait(false);
            }
            else
            {
                return await Context.Albums
                                    .Where(a => a.PerformerID == performer.ID)
                                    .OrderBy(a => a.ReleaseYear)
                                    .ThenBy(a => a.Name)
                                    .ToListAsync()
                                    .ConfigureAwait(false);
            }
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
                        .Where(
                            p =>
                                string.Compare(p.Name.ToLower().Substring(0, 1), "a", StringComparison.Ordinal) < 0 ||
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
