using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MusCat.Core.Entities
{
    public class Song : IDataErrorInfo
    {
        public long Id { get; set; }
        public long AlbumId { get; set; }
        public byte TrackNo { get; set; }
        public string Name { get; set; }
        public string TimeLength { get; set; }
        public byte? Rate { get; set; }
        public Album Album { get; set; }

        #region IDataErrorInfo methods

        public const int MaxNameLength = 50;

        public string Error => string.Join("\n", this["Name"], this["TimeLength"]);

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
                        if (Name.Length > MaxNameLength)
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
    }
}
