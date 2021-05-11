using System.Threading.Tasks;

namespace MusCat.Core.Interfaces.Networking
{
    public interface IVideoLinkWebLoader
    {
        Task<string[]> LoadVideoLinksAsync(string performer, string song, int linkCount = 5);
    }
}
