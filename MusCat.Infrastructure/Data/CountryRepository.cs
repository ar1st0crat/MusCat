using System.Linq;
using MusCat.Core.Entities;

namespace MusCat.Infrastructure.Data
{
    class CountryRepository : Repository<Country>
    {
        public CountryRepository(MusCatDbContext context) : base(context)
        {
        }

        public override void Add(Country entity)
        {
            // manual autoincrement
            var lastId = Context.Countries.Select(s => (int)s.ID).DefaultIfEmpty(0).Max();
            entity.ID = (byte)++lastId;
            base.Add(entity);
        }
    }
}
