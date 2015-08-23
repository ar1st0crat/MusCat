using MusCatalog.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MusCatalog.ViewModel
{
    public class AlbumViewModel : INotifyPropertyChanged
    {
        public Album Album { get; set; }

        private ObservableCollection<Song> songs;
        public ObservableCollection<Song> Songs
        {
            get { return songs; }
            set
            {
                songs = value;
                RaisePropertyChanged("Songs");
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
