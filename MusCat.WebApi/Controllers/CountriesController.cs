using MusCat.Application.Dto;
using MusCat.Application.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace MusCat.WebApi.Controllers
{
    public class CountriesController : ApiController
    {
        private ICountryService _countryService;

        public CountriesController(ICountryService countryService)
        {
            _countryService = countryService;
        }

        // GET: api/Countries
        public async Task<IEnumerable<CountryDto>> Get()
        {
            return await _countryService.GetAllCountriesAsync();
        }

        // POST: api/Countries
        public async Task<Result<CountryDto>> Post([FromBody]string name)
        {
            return await _countryService.AddCountryAsync(name);
        }

        // PUT: api/Countries/5
        public async Task<Result<CountryDto>> Put(int id, [FromBody]string name)
        {
            return await _countryService.UpdateCountryAsync(id, name);
        }

        // DELETE: api/Countries/5
        public async Task<Result<CountryDto>> Delete(int id)
        {
            return await _countryService.RemoveCountryAsync(id);
        }
    }
}
