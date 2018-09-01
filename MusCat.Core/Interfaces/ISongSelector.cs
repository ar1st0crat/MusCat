using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces
{
    public interface ISongSelector
    {
        Song SelectSong();
        Task<Song> SelectSongAsync();
    }
}