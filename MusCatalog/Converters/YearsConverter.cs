using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using MusCatalog.Model;

namespace MusCatalog.Converters
{
    /// <summary>
    /// For a particular performer YearsConverter yields string of active years, e.g. "1970 - 2010"
    /// </summary>
    class YearsConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var performer = value as Performer;

            if (performer == null || performer.Albums.Count == 0)
            {
                return "";
            }
            
            var yearStart = performer.Albums.Min(t => t.ReleaseYear);
            var yearEnd = performer.Albums.Max(t => t.ReleaseYear);

            return yearEnd != yearStart ? 
                string.Format("{0} - {1}", yearStart, yearEnd) : yearStart.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
