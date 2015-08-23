using MusCatalog.Model;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace MusCatalog.ViewModel
{
    public class PerformerViewModel : INotifyPropertyChanged
    {
        public Performer Performer { get; set; }
        public byte? AlbumCollectionRate { get; set; }
        public int AlbumCount { get; set; }

        public AlbumViewModel SelectedAlbum { get; set; }

        private ObservableCollection<AlbumViewModel> albums = new ObservableCollection<AlbumViewModel>();
        public ObservableCollection<AlbumViewModel> Albums
        {
            get { return albums; }
            set
            {
                albums = value;
                RaisePropertyChanged("Albums");
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
