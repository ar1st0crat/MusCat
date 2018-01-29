using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using MusCat.Core.Entities;

namespace MusCat.Infrastructure.Data
{
    class CountryRepository : Repository<Country>
    {
        public CountryRepository(MusCatDbContext context) : base(context)
        {
        }

        public override async Task AddAsync(Country country)
        {
            // manual autoincrement
            var lastId = await Context.Countries
                                      .Select(s => (int)s.Id)
                                      .DefaultIfEmpty(0)
                                      .MaxAsync()
                                      .ConfigureAwait(false);

            country.Id = (byte)++lastId;
            Add(country);
        }
    }
}
