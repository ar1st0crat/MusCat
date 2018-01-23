namespace MusCat.Core.Entities
{
    public class Lineup
    {
        public long PerformerID { get; set; }
        public long MusicianID { get; set; }
        public short? YearStart { get; set; }
        public short? YearEnd { get; set; }
        public virtual Musician Musician { get; set; }
        public virtual Performer Performer { get; set; }
    }
}
