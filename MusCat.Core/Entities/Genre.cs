using System.Collections.Generic;

namespace MusCat.Core.Entities
{
    public class Genre
    {
        public byte ID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Performer> Performers { get; set; }

        public Genre()
        {
            Performers = new HashSet<Performer>();
        }
    }
}
