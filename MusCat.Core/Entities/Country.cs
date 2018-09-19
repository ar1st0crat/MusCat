using System.Collections.Generic;
using System.ComponentModel;

namespace MusCat.Core.Entities
{
    public class Country : IDataErrorInfo
    {
        public byte Id { get; set; }
        public string Name { get; set; }
        public ICollection<Performer> Performers { get; set; }
        
        public Country()
        {
            Performers = new HashSet<Performer>();
        }

        #region IDataErrorInfo methods

        public const int MaxNameLength = 20;

        public string Error => this["Name"];

        public string this[string columnName]
        {
            get
            {
                if (columnName != "Name")
                {
                    return string.Empty;
                }

                var error = string.Empty;

                if (string.IsNullOrWhiteSpace(Name))
                {
                    error = "Country name can't be empty";
                }
                else if (Name.Length > MaxNameLength)
                {
                    error = $"Country name should contain not more than {MaxNameLength} symbols";
                }
                
                return error;
            }
        }

        #endregion
    }
}
