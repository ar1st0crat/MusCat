using MusCat.Core.Entities;

namespace MusCat.ViewModels.Entities
{
    public class AlbumViewModel : ViewModelBase
    {
        public long Id { get; set; }
        public long PerformerId { get; set; }
        public string Name { get; set; }
        public short ReleaseYear { get; set; }
        public string TotalTime { get; set; }
        public string Info { get; set; }
        public byte? Rate { get; set; }

        public Performer Performer { get; set; }
    }
}
