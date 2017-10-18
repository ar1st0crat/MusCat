using System.Threading.Tasks;
using MusCat.Entities;

namespace MusCat.Services.Networking
{
    interface IWebDataLoader
    {
        Task<string> LoadBioAsync(Performer performer);
    }
}
