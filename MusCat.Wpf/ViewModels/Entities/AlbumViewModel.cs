using AutoMapper;
using MusCat.Core.Entities;
using System.ComponentModel;

namespace MusCat.ViewModels.Entities
{
    public class AlbumViewModel : ViewModelBase, IDataErrorInfo
    {
        public long Id { get; set; }
        public long PerformerId { get; set; }

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

        private short _releaseYear;
        public short ReleaseYear
        {
            get { return _releaseYear; }
            set
            {
                _releaseYear = value;
                RaisePropertyChanged();
            }
        }

        private string _totalTime;
        public string TotalTime
        {
            get { return _totalTime; }
            set
            {
                _totalTime = value;
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

        public Performer Performer { get; set; }

        #region IDataErrorInfo methods

        public string Error => Mapper.Map<Album>(this).Error;

        public string this[string columnName] => Mapper.Map<Album>(this)[columnName];

        #endregion
    }
}
