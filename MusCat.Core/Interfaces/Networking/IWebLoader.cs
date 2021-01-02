using System.Threading.Tasks;

namespace MusCat.Core.Interfaces.Networking
{
    public interface IBioWebLoader
    {
        Task<string> LoadBioAsync(string name);
    }
}
