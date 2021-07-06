using AutoMapper;
using MusCat.Application.Validators;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Services;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace MusCat.ViewModels.Entities
{
    public class PerformerViewModel : BindableBase, IDataErrorInfo
    {
        public int Id { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _info;
        public string Info
        {
            get { return _info; }
            set { SetProperty(ref _info, value); }
        }

        private Country _country;
        public Country Country
        {
            get { return _country; }
            set { SetProperty(ref _country, value); }
        }

        private byte? _albumCollectionRate;
        public byte? AlbumCollectionRate
        {
            get { return _albumCollectionRate; }
            set { SetProperty(ref _albumCollectionRate, value); }
        }
        
        private ObservableCollection<AlbumViewModel> _albums = new ObservableCollection<AlbumViewModel>();
        public ObservableCollection<AlbumViewModel> Albums
        {
            get { return _albums; }
            set { SetProperty(ref _albums, value); }
        }

        private string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set { SetProperty(ref _imagePath, value); }
        }

        private readonly object _lock = new object();


        public PerformerViewModel()
        {
            BindingOperations.EnableCollectionSynchronization(_albums, _lock);
        }

        public void LocateImagePath()
        {
            ImagePath = FileLocator.GetPerformerImagePath(Mapper.Map<Performer>(this));
        }


        #region IDataErrorInfo methods

        private readonly PerformerValidator _validator = new PerformerValidator();

        public string Error => this["Name"];

        public string this[string columnName]
        {
            get
            {
                var result = _validator.Validate(Mapper.Map<Performer>(this));

                if (!result.Errors.Any(e => e.PropertyName == columnName))
                {
                    return string.Empty;
                }

                var error = result.Errors
                                  .First(e => e.PropertyName == columnName)
                                  .ErrorMessage;
                return error;
            }
        }

        #endregion
    }
}
