using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MusCat.Core.Interfaces.Data;

namespace MusCat.Infrastructure.Data
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly MusCatDbContext Context;

        public Repository(MusCatDbContext context)
        {
            Context = context;
        }

        #region asynchronous functionality

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Context.Set<T>()
                                .ToListAsync()
                                .ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await Context.Set<T>()
                                .Where(predicate)
                                .ToListAsync()
                                .ConfigureAwait(false);
        }

        public virtual async Task<int> CountAsync(Expression<Func<T, bool>> predicate)
        {
            return await Context.Set<T>()
                                .CountAsync(predicate)
                                .ConfigureAwait(false);
        }

        #endregion

        #region synchronous functionality

        public virtual IEnumerable<T> GetAll()
        {
            return Context.Set<T>();
        }

        public IEnumerable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return Context.Set<T>().Where(predicate);
        }

        public virtual void Add(T entity)
        {
            Context.Set<T>().Add(entity);
        }

        public virtual void Delete(T entity)
        {
            Context.Set<T>().Remove(entity);
        }

        public virtual void Edit(T entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        #endregion
    }
}
