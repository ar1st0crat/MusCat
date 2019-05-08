using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MusCat.Core.Entities
{
    public class Song : IDataErrorInfo
    {
        public int Id { get; set; }
        public int AlbumId { get; set; }
        public byte TrackNo { get; set; }
        public string Name { get; set; }
        public string TimeLength { get; set; }
        public byte? Rate { get; set; }
        public Album Album { get; set; }

        #region IDataErrorInfo methods

        public const int MaxNameLength = 50;

        public string Error
        {
            get
            {
                var error = string.Join("\n", this["Name"], this["TimeLength"]);
                return error.Replace("\n", "") == string.Empty ? string.Empty : error;
            }
        }

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
                        if (string.IsNullOrWhiteSpace(Name))
                        {
                            error = "Song title can't be empty";
                        }
                        else if (Name.Length > MaxNameLength)
                        {
                            error = $"Song title should contain not more than {MaxNameLength} symbols";
                        }
                        break;
                    }
                }
                return error;
            }
        }

        #endregion
    }
}
