using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using MusCat.Entities;

namespace MusCat.Repositories.Base
{
    abstract class Repository<T> : IRepository<T> where T : class
    {
        protected readonly MusCatEntities Context;

        protected Repository(MusCatEntities context)
        {
            Context = context;
        }
        
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

        public virtual async Task<IEnumerable<T>> GetAllAsync()
        {
            return await Context.Set<T>().ToListAsync().ConfigureAwait(false);
        }

        public virtual async Task<IEnumerable<T>> GetAsync(Expression<Func<T, bool>> predicate)
        {
            return await Context.Set<T>().Where(predicate)
                                .ToListAsync().ConfigureAwait(false);
        }
        
        public virtual async Task AddAsync(T entity)
        {
            // default implementation is simply synchronous
            Add(entity);
        }
    }
}
