using System;
using System.Data.Entity;
using System.Linq;
using System.Linq.Expressions;

namespace MusCat.Repository.Base
{
    abstract class Repository<TContext, TEntity> : DisposableRepository<TContext>, IRepository<TEntity>
        where TEntity : class 
        where TContext : DbContext, new()
    {
        public virtual IQueryable<TEntity> GetAll()
        {
            return Context.Set<TEntity>();
        }

        public IQueryable<TEntity> FindBy(Expression<Func<TEntity, bool>> predicate)
        {
            return Context.Set<TEntity>().Where(predicate);
        }

        public virtual void Add(TEntity entity)
        {
            Context.Set<TEntity>().Add(entity);
        }

        public virtual void Delete(TEntity entity)
        {
            Context.Set<TEntity>().Remove(entity);
        }

        public virtual void Edit(TEntity entity)
        {
            Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Save()
        {
            Context.SaveChanges();
        }
    }
}
