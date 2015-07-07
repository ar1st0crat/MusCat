using MusCatalog.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;


namespace MusCatalog
{
    public class RateConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performers perf = value as Performers;

            if (perf != null)
            {
                if (perf.Albums.Count == 0)
                    return "Not rated";

                int ratedCount = perf.Albums.Count(t => t.ARate.HasValue);
                int rate = perf.Albums.Sum(t =>
                {
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

            Albums albs = value as Albums;

            if (albs != null)
            {
                if (albs.ARate.HasValue)
                    return albs.ARate + "/10";
                else
                    return "Not rated";
            }

            return "Not Rated";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
