using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Radio
{
    public interface ISongSelector
    {
        Song DefaultSong();

        Task<Song> SelectSongAsync();
    }
}
