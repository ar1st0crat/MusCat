using MusCat.Core.Interfaces.Tracklist;
using System.Threading.Tasks;

namespace MusCat.Core.Interfaces.Networking
{
    public interface IWebLoader
    {
        Task<string> LoadBioAsync(string name);
        Task<Track[]> LoadTracksAsync(string performer, string album);
    }
}
