using AutoMapper;
using MusCat.Application.Validators;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Services;
using Prism.Mvvm;
using System.ComponentModel;
using System.Linq;

namespace MusCat.ViewModels.Entities
{
    public class AlbumViewModel : BindableBase, IDataErrorInfo
    {
        public int Id { get; set; }
        public int PerformerId { get; set; }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private short _releaseYear;
        public short ReleaseYear
        {
            get { return _releaseYear; }
            set { SetProperty(ref _releaseYear, value); }
        }

        private string _totalTime;
        public string TotalTime
        {
            get { return _totalTime; }
            set { SetProperty(ref _totalTime, value); }
        }

        private string _info;
        public string Info
        {
            get { return _info; }
            set { SetProperty(ref _info, value); }
        }

        private byte? _rate;
        public byte? Rate
        {
            get { return _rate; }
            set { SetProperty(ref _rate, value); }
        }

        private string _imagePath;
        public string ImagePath
        {
            get { return _imagePath; }
            set { SetProperty(ref _imagePath, value); }
        }

        public Performer Performer { get; set; }

        public void LocateImagePath()
        {
            ImagePath = FileLocator.GetAlbumImagePath(Mapper.Map<Album>(this));
        }


        #region IDataErrorInfo methods

        private readonly AlbumValidator _validator = new AlbumValidator();

        public string Error
        {
            get
            {
                var error = string.Join("\n", this["Name"], this["TotalTime"]);
                return error.Replace("\n", "") == string.Empty ? string.Empty : error;
            }
        }

        public string this[string columnName]
        {
            get
            {
                var result = _validator.Validate(Mapper.Map<Album>(this));

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
