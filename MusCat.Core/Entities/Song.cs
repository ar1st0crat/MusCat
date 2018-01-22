using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MusCat.Core.Entities
{
    public class Song : INotifyPropertyChanged, IDataErrorInfo
    {
        public long AlbumID { get; set; }

        //[DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long ID { get; set; }

        public byte TrackNo
        {
            get { return _trackNo; }
            set
            {
                _trackNo = value;
                RaisePropertyChanged("TrackNo");
            }
        }

        //[Required]
        //[StringLength(50)]
        public string Name
        {
            get { return _name; }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }
        
        //[Required]
        //[StringLength(6)]
        public string TimeLength
        {
            get { return _timeLength; }
            set
            {
                _timeLength = value;
                RaisePropertyChanged("TimeLength");
            }
        }

        public byte? Rate
        {
            get { return _rate; }
            set
            {
                _rate = value;
                RaisePropertyChanged("Rate");
            }
        }

        private byte _trackNo;
        private string _name;
        private string _timeLength;
        private byte? _rate;

        public virtual Album Album { get; set; }

        #region IDataErrorInfo methods

        public string Error => this["Name"] + this["TimeLength"];

        public string this[string columnName]
        {
            get
            {
                var error = string.Empty;

                switch (columnName)
                {
                    case "TimeLength":
                    {
                        var regex = new Regex(@"^\d+:\d{2}$");
                    
                        if (TimeLength == null || !regex.IsMatch(TimeLength))
                        {
                            error = "Time length should be in the format mm:ss";
                        }
                        break;
                    }
                    case "Name":
                    {
                        if (Name.Length > 50)
                        {
                            error = "Song title should contain not more than 50 symbols";
                        }
                        else if (Name == "")
                        {
                            error = "Song title can't be empty";
                        }
                        break;
                    }
                }
                return error;
            }
        }

        #endregion

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

    }
}
