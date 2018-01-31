using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces
{
    public interface ISongSelector
    {
        Task<Song> SelectSongAsync();
    }
}