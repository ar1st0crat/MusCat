using System;
using System.Data.Entity;

namespace MusCat.Repositories.Base
{
    class DisposableRepository<TContext> : IDisposable
        where TContext : DbContext, new()
    {
        protected TContext Context { get; set; } = new TContext();

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Context.Dispose();
                }
            }

            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
