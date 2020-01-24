namespace MusCat.Core.Entities
{
    public class Song
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public byte TrackNo { get; set; }
        public string Name { get; set; }
        public string TimeLength { get; set; }
        public byte? Rate { get; set; }
        public Album Album { get; set; }
    }
}
