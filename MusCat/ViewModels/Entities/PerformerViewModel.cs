using System.Collections.ObjectModel;
using System.Windows.Data;
using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Services;

namespace MusCat.ViewModels.Entities
{
    public class PerformerViewModel : ViewModelBase
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public Country Country { get; set; }

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
            var albums = Mapper.Map<Album[]>(_albums);

            AlbumCollectionRate = _rateCalculator.Calculate(albums);
        }

        private readonly object _lock = new object();

        public PerformerViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_albums, _lock);
        }
    }
}
