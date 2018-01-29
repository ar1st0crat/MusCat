using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Services;

namespace MusCat.ViewModels.Entities
{
    public class PerformerViewModel : ViewModelBase
    {
        public long Id { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged();
            }
        }

        private string _info;
        public string Info
        {
            get { return _info; }
            set
            {
                _info = value;
                RaisePropertyChanged();
            }
        }

        private Country _country;
        public Country Country
        {
            get { return _country; }
            set
            {
                _country = value;
                RaisePropertyChanged();
            }
        }

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

        public int AlbumCount => _albums.Count;

        public AlbumViewModel SelectedAlbum { get; set; }

        private readonly IRateCalculator _rateCalculator = new RateCalculator();

        public void UpdateAlbumCollectionRate()
        {
            var rates = _albums.Select(a => a.Rate);
            AlbumCollectionRate = _rateCalculator.Calculate(rates);
        }

        private readonly object _lock = new object();

        public PerformerViewModel()//IRateCalculator rateCalculator)
        {
            //_rateCalculator = rateCalculator;
            BindingOperations.EnableCollectionSynchronization(_albums, _lock);
        }
    }
}
