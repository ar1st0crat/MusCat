using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Core.Interfaces.Domain
{
    public interface ICountryService
    {
        Task<long> GetPerformersCountAsync(byte countryId);
        Task<Result<Country>> AddCountryAsync(string name);
        Task<Result<Country>> RemoveCountryAsync(byte countryId);
        Task<Result<Country>> UpdateCountryAsync(byte countryId, string name);
    }
}