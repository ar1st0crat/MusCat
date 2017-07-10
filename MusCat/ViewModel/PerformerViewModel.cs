using System.Collections.ObjectModel;
using System.ComponentModel;
using MusCat.Model;

namespace MusCat.ViewModel
{
    public class PerformerViewModel : INotifyPropertyChanged
    {
        public Performer Performer { get; set; }

        private byte? albumCollectionRate;
        public byte? AlbumCollectionRate
        {
            get { return albumCollectionRate; }
            set
            {
                albumCollectionRate = value;
                RaisePropertyChanged("AlbumCollectionRate");
            }
        }

        private int albumCount;
        public int AlbumCount
        {
            get { return albumCount; }
            set
            {
                albumCount = value;
                RaisePropertyChanged("AlbumCount");
            }
        }

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
