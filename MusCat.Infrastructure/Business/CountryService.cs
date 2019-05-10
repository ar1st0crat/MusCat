﻿using System.Linq;
using System.Threading.Tasks;
using MusCat.Application.Dto;
using MusCat.Application.Interfaces;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Data;
using MusCat.Core.Util;

namespace MusCat.Infrastructure.Business
{
    public class CountryService : ICountryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CountryService(IUnitOfWork unitOfWork)
        {
            Guard.AgainstNull(unitOfWork);
            _unitOfWork = unitOfWork;
        }

        public async Task<int> GetPerformersCountAsync(int countryId)
        {
            return await _unitOfWork.PerformerRepository
                                    .CountAsync(p => p.CountryId == countryId)
                                    .ConfigureAwait(false);
        }

        public async Task<Result<CountryDto>> AddCountryAsync(string name)
        {
            var country = new Country { Name = name };

            if (country.Error != string.Empty)
            {
                return new Result<CountryDto>(ResultType.Invalid, country.Error);
            }

            var duplicates = await _unitOfWork.CountryRepository
                                              .GetAsync(c => c.Name == name)
                                              .ConfigureAwait(false);

            if (duplicates.Any())
            {
                return new Result<CountryDto>(ResultType.Invalid, 
                    "The specified country is already in the list!");
            }

            _unitOfWork.CountryRepository.Add(country);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<CountryDto>(Auto.Mapper.Map<CountryDto>(country));
        }

        public async Task<Result<CountryDto>> RemoveCountryAsync(int countryId)
        {
            var countries = await _unitOfWork.CountryRepository
                                             .GetAsync(c => c.Id == countryId)
                                             .ConfigureAwait(false);

            var country = countries.FirstOrDefault();

            if (country == null)
            {
                return new Result<CountryDto>(ResultType.Invalid, "Could not find country!");
            }

            _unitOfWork.CountryRepository.Delete(country);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<CountryDto>(Auto.Mapper.Map<CountryDto>(country));
        }

        public async Task<Result<CountryDto>> UpdateCountryAsync(int countryId, string name)
        {
            var country = new Country { Name = name };

            if (country.Error != string.Empty)
            {
                return new Result<CountryDto>(ResultType.Invalid, country.Error);
            }

            var countries = await _unitOfWork.CountryRepository
                                             .GetAsync(c => c.Id == countryId)
                                             .ConfigureAwait(false);

            country = countries.FirstOrDefault();

            if (country == null)
            {
                return new Result<CountryDto>(ResultType.Invalid, "Could not find country!");
            }

            country.Name = name;
            _unitOfWork.CountryRepository.Edit(country);
            await _unitOfWork.SaveAsync().ConfigureAwait(false);

            return new Result<CountryDto>(Auto.Mapper.Map<CountryDto>(country));
        }
    }
}