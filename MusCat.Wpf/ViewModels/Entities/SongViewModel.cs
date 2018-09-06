using AutoMapper;
using MusCat.Core.Entities;
using System.ComponentModel;

namespace MusCat.ViewModels.Entities
{
    public class SongViewModel : ViewModelBase, IDataErrorInfo
    {
        public long Id { get; set; }
        public long AlbumId { get; set; }

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

        public string Error => Mapper.Map<Song>(this).Error;

        public string this[string columnName] => Mapper.Map<Song>(this)[columnName];
    }
}
