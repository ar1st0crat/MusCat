using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Radio
{
    public interface ISongSelector
    {
        Song DefaultSong();
        Song SelectSong();
        Task<Song> SelectSongAsync();
    }
}