using System;
using MusCat.Entities;

namespace MusCat.Repositories.Base
{
    class UnitOfWork : IDisposable
    {
        private readonly MusCatEntities _context = new MusCatEntities();

        private IPerformerRepository _performerRepository;
        private IRepository<Album> _albumRepository;
        private IRepository<Song> _songRepository;

        public IPerformerRepository PerformerRepository => 
            _performerRepository ?? (_performerRepository = new PerformerRepository(_context));

        public IRepository<Album> AlbumRepository => 
            _albumRepository ?? (_albumRepository = new AlbumRepository(_context));

        public IRepository<Song> SongRepository =>
            _songRepository ?? (_songRepository = new SongRepository(_context));


        public void Save()
        {
            _context.SaveChanges();
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
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
