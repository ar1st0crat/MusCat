using System.Collections.Generic;
using System.ComponentModel;

namespace MusCat.Core.Entities
{
    public class Performer : IDataErrorInfo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Info { get; set; }
        public byte? CountryId { get; set; }
        public ICollection<Album> Albums { get; set; }
        public Country Country { get; set; }
        public ICollection<Lineup> Lineups { get; set; }
        public ICollection<Genre> Genres { get; set; }

        public Performer()
        {
            Albums = new HashSet<Album>();
            Lineups = new HashSet<Lineup>();
            Genres = new HashSet<Genre>();
        }
        
        #region IDataErrorInfo methods

        public const int MaxNameLength = 30;

        public string Error => this["Name"];

        public string this[string columnName]
        {
            get
            {
                var error = string.Empty;

                switch (columnName)
                {
                    case "Name":
                        {
                            if (string.IsNullOrWhiteSpace(Name))
                            {
                                error = "Performer name can't be empty";
                            }
                            else if (Name.Length > MaxNameLength)
                            {
                                error = $"Performer name should contain not more than {MaxNameLength} symbols";
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
