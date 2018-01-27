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

        public string Error => string.Join("\n", this["Name"], this["TotalTime"]);

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
                            if (Name.Length > MaxNameLength)
                            {
                                error = "Album name should contain not more than 40 symbols";
                            }
                            else if (Name == "")
                            {
                                error = "Album name can't be empty";
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
