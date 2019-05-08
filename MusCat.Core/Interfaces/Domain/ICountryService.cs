using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Domain
{
    public interface ICountryService
    {
        Task<int> GetPerformersCountAsync(int countryId);
        Task<Result<Country>> AddCountryAsync(string name);
        Task<Result<Country>> RemoveCountryAsync(int countryId);
        Task<Result<Country>> UpdateCountryAsync(int countryId, string name);
    }
}