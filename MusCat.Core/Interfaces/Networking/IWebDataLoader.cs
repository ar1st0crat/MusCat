using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Networking
{
    public interface IWebDataLoader
    {
        Task<string> LoadBioAsync(Performer performer);
    }
}
