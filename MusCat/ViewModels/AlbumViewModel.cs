using System.Collections.ObjectModel;
using System.ComponentModel;
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

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
