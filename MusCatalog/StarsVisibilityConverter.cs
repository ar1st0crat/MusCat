using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace MusCatalog
{
    class StarsVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performers perf = value as Performers;

            if (perf != null)
            {
                if (perf.Albums.Count == 0)
                    return "Hidden";

                int ratedCount = perf.Albums.Count(t => t.ARate.HasValue);
                int rate = perf.Albums.Sum(t =>
                {
                    if (t.ARate.HasValue)
                        return t.ARate.Value;
                    else
                        return 0;
                });

                if (ratedCount == 0)
                    return "Hidden";
                else
                    return "Visible";
            }

            Albums albs = value as Albums;

            if (albs != null)
            {
                if (albs.ARate.HasValue)
                    return "Visible";
                else
                    return "Hidden";
            }

            return "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
