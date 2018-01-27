using System.Collections.Generic;

namespace MusCat.Core.Entities
{
    public class Genre
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public ICollection<Performer> Performers { get; set; }

        public Genre()
        {
            Performers = new HashSet<Performer>();
        }
    }
}
