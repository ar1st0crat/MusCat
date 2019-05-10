using System.Threading.Tasks;
using MusCat.Application.Dto;

namespace MusCat.Application.Interfaces
{
    public interface ICountryService
    {
        Task<int> GetPerformersCountAsync(int countryId);
        Task<Result<CountryDto>> AddCountryAsync(string name);
        Task<Result<CountryDto>> RemoveCountryAsync(int countryId);
        Task<Result<CountryDto>> UpdateCountryAsync(int countryId, string name);
    }
}