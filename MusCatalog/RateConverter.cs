using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MusCatalog
{
    public class RateConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performers perf = value as Performers;

            if (perf == null)
                return "";

            short ratedCount = 0;
            byte rate = 0;

            foreach (var a in perf.Albums)
                if (a.ARate.HasValue)
                {
                    ratedCount++;
                    rate += a.ARate.Value;
                }

            if (ratedCount == 0)
                return "Not rated";
            else
                return rate / ratedCount + "/10";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
