using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MusCatalog
{
    class YearsConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performers perf = value as Performers;

            if (perf == null)
                return "";

            if (perf.Albums.Count == 0)
                return "";

            short yearStart = perf.Albums.Min( t => t.AYear );
            short yearEnd = perf.Albums.Max( t => t.AYear );

            if (yearEnd != yearStart)
                return string.Format("{0} - {1}", yearStart, yearEnd);
            else
                return yearStart.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
