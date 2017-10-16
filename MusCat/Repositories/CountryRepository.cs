using System.Linq;
using MusCat.Entities;
using MusCat.Repositories.Base;

namespace MusCat.Repositories
{
    class CountryRepository : Repository<Country>
    {
        public CountryRepository(MusCatEntities context) : base(context)
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
