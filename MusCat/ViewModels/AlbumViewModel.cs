using System.Collections.ObjectModel;
using System.Windows.Data;
using MusCat.Core.Entities;

namespace MusCat.ViewModels
{
    public class AlbumViewModel : ViewModelBase
    {
        private Album _album; 
        public Album Album
        {
            get { return _album; }
            set
            {
                _album = value;
                RaisePropertyChanged();
            }
        }

        private ObservableCollection<Song> _songs = new ObservableCollection<Song>();
        public ObservableCollection<Song> Songs
        {
            get { return _songs; }
            set
            {
                _songs = value;
                RaisePropertyChanged();
            }
        }

        private readonly object _lock = new object();

        public AlbumViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_songs, _lock);
        }
    }
}
