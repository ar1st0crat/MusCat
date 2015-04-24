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

            if (perf.Albums.Count == 0)
                return "Not rated";

            int ratedCount = perf.Albums.Count( t => t.ARate.HasValue );
            int rate = perf.Albums.Sum( t => {
                                                if (t.ARate.HasValue)
                                                    return t.ARate.Value;
                                                else
                                                    return 0;
                                        });

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
