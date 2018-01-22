using System.Collections.Generic;

namespace MusCat.Core.Entities
{
    public class Performer
    {
        public Performer()
        {
            Albums = new HashSet<Album>();
            Lineups = new HashSet<Lineup>();
            Genres = new HashSet<Genre>();
        }

        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ID { get; set; }

        //[Required]
        //[StringLength(30)]
        public string Name { get; set; }

        //[Column(TypeName = "text")]
        public string Info { get; set; }

        public byte? CountryID { get; set; }

        public virtual ICollection<Album> Albums { get; set; }

        public virtual Country Country { get; set; }

        public virtual ICollection<Lineup> Lineups { get; set; }

        public virtual ICollection<Genre> Genres { get; set; }
    }
}
