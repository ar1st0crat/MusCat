namespace MusCat.ViewModels.Entities
{
    class SongViewModel : ViewModelBase
    {
        public long Id { get; set; }
        public long AlbumId { get; set; }
        public byte TrackNo { get; set; }
        public string Name { get; set; }
        public string TimeLength { get; set; }
        public byte? Rate { get; set; }
    }
}
