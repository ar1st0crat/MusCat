using AutoMapper;
using MusCat.Application.Validators;
using MusCat.Core.Entities;
using Prism.Mvvm;
using System.ComponentModel;
using System.Linq;

namespace MusCat.ViewModels.Entities
{
    public class SongViewModel : BindableBase, IDataErrorInfo
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }

        private byte _trackNo;
        public byte TrackNo
        {
            get { return _trackNo; }
            set { SetProperty(ref _trackNo, value); }
        }

        private string _name;
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        private string _timeLength;
        public string TimeLength
        {
            get { return _timeLength; }
            set { SetProperty(ref _timeLength, value); }
        }

        private byte? _rate;
        public byte? Rate
        {
            get { return _rate; }
            set { SetProperty(ref _rate, value); }
        }


        #region IDataErrorInfo methods

        private readonly SongValidator _validator = new SongValidator();

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
                var result = _validator.Validate(Mapper.Map<Song>(this));

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
