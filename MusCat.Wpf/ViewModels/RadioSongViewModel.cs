using MusCat.Core.Entities;
using MusCat.Infrastructure.Services;

namespace MusCat.ViewModels
{
    public class RadioSongViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public string TimeLength { get; set; }
        public Album Album { get; set; }
        public string AlbumImagePath { get; set; }

        public void LocateAlbumImagePath(Album album)
        {
            AlbumImagePath = FileLocator.GetAlbumImagePath(album);
        }
    }
}
