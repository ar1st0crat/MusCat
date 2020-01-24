using System.Collections.Generic;

namespace MusCat.Core.Entities
{
    public class Performer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public int? CountryId { get; set; }
        public ICollection<Album> Albums { get; set; }
        public Country Country { get; set; }
        public ICollection<Lineup> Lineups { get; set; }
        public ICollection<Genre> Genres { get; set; }

        public Performer()
        {
            Albums = new HashSet<Album>();
            Lineups = new HashSet<Lineup>();
            Genres = new HashSet<Genre>();
        }
    }
}
