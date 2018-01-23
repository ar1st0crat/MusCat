using System;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;

namespace MusCat.Infrastructure.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly MusCatDbContext _context = new MusCatDbContext();

        private IPerformerRepository _performerRepository;
        private IAlbumRepository _albumRepository;
        private IRepository<Song> _songRepository;
        private IRepository<Country> _countryRepository;

        public IPerformerRepository PerformerRepository => 
            _performerRepository ?? (_performerRepository = new PerformerRepository(_context));

        public IAlbumRepository AlbumRepository => 
            _albumRepository ?? (_albumRepository = new AlbumRepository(_context));

        public IRepository<Song> SongRepository =>
            _songRepository ?? (_songRepository = new SongRepository(_context));

        public IRepository<Country> CountryRepository =>
            _countryRepository ?? (_countryRepository = new CountryRepository(_context));


        public void Save()
        {
            _context.SaveChanges();
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync().ConfigureAwait(false);
        }

        #region Dispose pattern

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

        #endregion
    }
}
