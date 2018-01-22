using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Data
{
    public interface IUnitOfWork
    {
        IPerformerRepository PerformerRepository { get; }
        IAlbumRepository AlbumRepository { get; }
        IRepository<Song> SongRepository { get; }
        IRepository<Country> CountryRepository { get; }
        void Save();
        Task SaveAsync();
    }
}