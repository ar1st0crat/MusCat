using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;

namespace MusCat.Core.Services
{
    public class CountryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CountryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Country>> GetAllCountriesAsync()
        {
            return await _unitOfWork.CountryRepository
                                    .GetAllAsync()
                                    .ConfigureAwait(false);
        }

        public Result<Country> AddCountry(string name)
        {
            var country = new Country { Name = name };

            if (country.Error != string.Empty)
            {
                return new Result<Country>(ResultType.Invalid, country.Error);
            }

            var duplicates = _unitOfWork.CountryRepository.Get(c => c.Name == name);

            if (duplicates.Any())
            {
                return new Result<Country>(ResultType.Invalid, 
                    "The specified country is already in the list!");
            }

            _unitOfWork.CountryRepository.Add(country);
            _unitOfWork.Save();

            return new Result<Country>(country);
        }

        public Result<Country> RemoveCountry(byte countryId)
        {
            var country = _unitOfWork.CountryRepository
                                     .Get(c => c.Id == countryId)
                                     .FirstOrDefault();
            if (country == null)
            {
                return new Result<Country>(ResultType.Invalid, "Could not find country!");
            }

            _unitOfWork.CountryRepository.Delete(country);
            _unitOfWork.Save();

            return new Result<Country>(country);
        }

        public Result<Country> UpdateCountry(byte countryId, string name)
        {
            var country = new Country { Name = name };

            if (country.Error != string.Empty)
            {
                return new Result<Country>(ResultType.Invalid, country.Error);
            }

            country = _unitOfWork.CountryRepository
                                 .Get(c => c.Id == countryId)
                                 .FirstOrDefault();

            if (country == null)
            {
                return new Result<Country>(ResultType.Invalid, "Could not find country!");
            }

            country.Name = name;
            _unitOfWork.CountryRepository.Edit(country);
            _unitOfWork.Save();

            return new Result<Country>(country);
        }
    }
}
