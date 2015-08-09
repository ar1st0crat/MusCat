using MusCatalog.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;


namespace MusCatalog
{
    class StarsVisibilityConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performer perf = value as Performer;

            if (perf != null)
            {
                if (perf.Albums.Count == 0)
                {
                    return "Hidden";
                }

                int ratedCount = perf.Albums.Count(t => t.Rate.HasValue);

                if (ratedCount == 0)
                {
                    return "Hidden";
                }
                else
                {
                    return "Visible";
                }
            }

            Album albs = value as Album;

            if (albs != null)
            {
                if (albs.Rate.HasValue)
                {
                    return "Visible";
                }
                else
                {
                    return "Hidden";
                }
            }

            return "Hidden";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
