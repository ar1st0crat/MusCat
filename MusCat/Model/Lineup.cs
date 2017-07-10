namespace MusCat.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Lineup
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long PerformerID { get; set; }

        [Key]
        [Column(Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long MusicianID { get; set; }

        public short? YearStart { get; set; }

        public short? YearEnd { get; set; }

        public virtual Musician Musician { get; set; }

        public virtual Performer Performer { get; set; }
    }
}
