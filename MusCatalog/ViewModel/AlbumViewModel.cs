using MusCatalog.Model;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MusCatalog.ViewModel
{
    public class AlbumViewModel : INotifyPropertyChanged
    {
        private Album album; 
        public Album Album
        {
            get { return album; }
            set
            {
                album = value;
                RaisePropertyChanged( "Album" );
            }
        }

        private ObservableCollection<Song> songs = new ObservableCollection<Song>();
        public ObservableCollection<Song> Songs
        {
            get { return songs; }
            set
            {
                songs = value;
                RaisePropertyChanged("Songs");
            }
        }

        public void LoadSongs()
        {
            // load and prepare all songs from the album for further actions
            using (var context = new MusCatEntities())
            {
                Songs.Clear();

                var albumSongs = context.Songs.Where(s => s.AlbumID == Album.ID).ToList();

                foreach (var song in albumSongs)
                {
                    song.Album = Album;
                    songs.Add(song);
                }
            }
        }


        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
