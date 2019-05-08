namespace MusCat.Core.Entities
{
    public class Lineup
    {
        public int PerformerId { get; set; }
        public int MusicianId { get; set; }
        public short? YearStart { get; set; }
        public short? YearEnd { get; set; }
        public Musician Musician { get; set; }
        public Performer Performer { get; set; }
    }
}
