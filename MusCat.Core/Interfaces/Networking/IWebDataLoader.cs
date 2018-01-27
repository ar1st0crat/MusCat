using System.Threading.Tasks;

namespace MusCat.Core.Interfaces.Networking
{
    public interface IWebDataLoader
    {
        Task<string> LoadBioAsync(string name);
    }
}
