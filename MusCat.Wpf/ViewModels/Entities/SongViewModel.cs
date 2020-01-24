using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Infrastructure.Validators;
using System.ComponentModel;
using System.Linq;

namespace MusCat.ViewModels.Entities
{
    public class SongViewModel : ViewModelBase, IDataErrorInfo
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }

        private byte _trackNo;
        public byte TrackNo
        {
            get { return _trackNo; }
            set
            {
                _trackNo = value;
                RaisePropertyChanged();
            }
        }

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

        private string _timeLength;
        public string TimeLength
        {
            get { return _timeLength; }
            set
            {
                _timeLength = value;
                RaisePropertyChanged();
            }
        }

        private byte? _rate;
        public byte? Rate
        {
            get { return _rate; }
            set
            {
                _rate = value;
                RaisePropertyChanged();
            }
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

                var error = result.Errors
                                  .First(e => e.PropertyName == columnName)
                                  .ErrorMessage;
                return error;
            }
        }

        #endregion
    }
}
