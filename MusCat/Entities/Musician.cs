using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MusCat.Entities
{
    public class Musician
    {
        public Musician()
        {
            Lineups = new HashSet<Lineup>();
        }

        public long ID { get; set; }

        [Required]
        [StringLength(40)]
        public string Name { get; set; }

        [Column(TypeName = "text")]
        public string Info { get; set; }

        public short? YearBorn { get; set; }

        public short? YearDied { get; set; }

        public long? PerformerID { get; set; }

        public virtual ICollection<Lineup> Lineups { get; set; }
    }
}
