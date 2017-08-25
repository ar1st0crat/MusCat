using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using MusCat.Entities;

namespace MusCat.ViewModels
{
    public class AlbumViewModel : INotifyPropertyChanged
    {
        private Album _album; 
        public Album Album
        {
            get { return _album; }
            set
            {
                _album = value;
                RaisePropertyChanged("Album");
            }
        }

        private ObservableCollection<Song> _songs = new ObservableCollection<Song>();
        public ObservableCollection<Song> Songs
        {
            get { return _songs; }
            set
            {
                _songs = value;
                RaisePropertyChanged("Songs");
            }
        }

        private readonly object _lock = new object();

        public AlbumViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_songs, _lock);
        }

        /// <summary>
        /// Load songs lazily
        /// </summary>
        public void LoadSongs()
        {
            // load and prepare all songs from the album for further actions
            using (var context = new MusCatEntities())
            {
                Songs.Clear();

                var albumSongs = context.Songs
                                        .Where(s => s.AlbumID == Album.ID)
                                        .ToList();

                foreach (var song in albumSongs)
                {
                    song.Album = Album;
                    _songs.Add(song);
                }
            }
        }

        /// <summary>
        /// Async version of LoadSongs()
        /// </summary>
        public async Task LoadSongsAsync()
        {
            // load and prepare all songs from the album for further actions
            using (var context = new MusCatEntities())
            {
                Songs.Clear();

                var albumSongs = await 
                        context.Songs.Where(s => s.AlbumID == Album.ID)
                                     .ToListAsync()
                                     .ConfigureAwait(false);

                foreach (var song in albumSongs)
                {
                    song.Album = Album;
                    _songs.Add(song);
                }
            }
        }

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
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
