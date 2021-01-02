using System.Threading.Tasks;

namespace MusCat.Core.Interfaces.Networking
{
    public interface ILyricsWebLoader
    {
        Task<string> LoadLyricsAsync(string performer, string song);
    }
}
