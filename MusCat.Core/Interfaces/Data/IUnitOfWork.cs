using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Data
{
    public interface IUnitOfWork
    {
        IPerformerRepository PerformerRepository { get; }
        IRepository<Album> AlbumRepository { get; }
        IRepository<Song> SongRepository { get; }
        IRepository<Country> CountryRepository { get; }
        void Save();
        Task SaveAsync();
    }
}