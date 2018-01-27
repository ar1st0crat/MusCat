using System.Collections.Generic;

namespace MusCat.Core.Entities
{
    public class Musician
    {
        public long Id { get; set; }
        public long? PerformerId { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public short? YearBorn { get; set; }
        public short? YearDied { get; set; }
        public ICollection<Lineup> Lineups { get; set; }

        public Musician()
        {
            Lineups = new HashSet<Lineup>();
        }
    }
}
