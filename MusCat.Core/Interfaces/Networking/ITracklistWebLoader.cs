using MusCat.Core.Interfaces.Tracklist;
using System.Threading.Tasks;

namespace MusCat.Core.Interfaces.Networking
{
    public interface ITracklistWebLoader
    {
        Task<Track[]> LoadTracksAsync(string performer, string album);
    }
}
