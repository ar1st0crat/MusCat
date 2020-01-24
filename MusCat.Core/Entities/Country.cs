using System.Collections.Generic;

namespace MusCat.Core.Entities
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<Performer> Performers { get; set; }
        
        public Country()
        {
            Performers = new HashSet<Performer>();
        }
    }
}
