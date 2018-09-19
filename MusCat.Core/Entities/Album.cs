using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace MusCat.Core.Entities
{
    public class Album : IDataErrorInfo
    {
        public long Id { get; set; }
        public long PerformerId { get; set; }
        public string Name { get; set; }
        public short ReleaseYear { get; set; }
        public string TotalTime { get; set; }
        public string Info { get; set; }
        public byte? Rate { get; set; }
        public Performer Performer { get; set; }
        public ICollection<Song> Songs { get; set; }

        public Album()
        {
            Songs = new HashSet<Song>();
        }

        #region IDataErrorInfo methods

        public const int MaxNameLength = 40;

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
                var error = string.Empty;

                switch (columnName)
                {
                    case "TotalTime":
                        {
                            var regex = new Regex(@"^\d+:\d{2}$");
                            if (!regex.IsMatch(TotalTime))
                            {
                                error = "Total time should be in the format mm:ss";
                            }
                            break;
                        }
                    case "Name":
                        {
                            if (string.IsNullOrWhiteSpace(Name))
                            {
                                error = "Album name can't be empty";
                            }
                            else if (Name.Length > MaxNameLength)
                            {
                                error = $"Album name should contain not more than {MaxNameLength} symbols";
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
