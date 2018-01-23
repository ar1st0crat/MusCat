using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using MusCat.Core.Entities;
using MusCat.Core.Services;

namespace MusCat.ViewModels
{
    public class PerformerViewModel : ViewModelBase
    {
        public Performer Performer { get; set; }

        private byte? _albumCollectionRate;
        public byte? AlbumCollectionRate
        {
            get { return _albumCollectionRate; }
            set
            {
                _albumCollectionRate = value;
                RaisePropertyChanged();
            }
        }

        private int _albumCount;
        public int AlbumCount
        {
            get { return _albumCount; }
            set
            {
                _albumCount = value;
                RaisePropertyChanged();
            }
        }

        public AlbumViewModel SelectedAlbum { get; set; }

        private ObservableCollection<AlbumViewModel> _albums = new ObservableCollection<AlbumViewModel>();
        public ObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set
            {
                _albums = value;
                RaisePropertyChanged();
            }
        }

        private readonly RateCalculator _rateCalculator = new RateCalculator();

        public void UpdateAlbumCollectionRate()
        {
            var albums = _albums.Select(a => a.Album);

            AlbumCollectionRate = _rateCalculator.Calculate(albums);
        }

        private readonly object _lock = new object();

        public PerformerViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_albums, _lock);
        }
    }
}
