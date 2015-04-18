using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MusCatalog
{
    public class RateStarsConverter: IValueConverter
    {
        private string getStar( byte rate, int nStar )
        {
            if (rate >= 2 * nStar)
                return @"Images\star.png";

            else if (rate < 2*nStar && 2*nStar - rate == 1)
                return @"Images\star_half.png";

            else
                return @"Images\star_empty.png";
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performers perf = value as Performers;

            if (perf != null)
            {
                short ratedCount = 0;
                byte rate = 0;

                foreach (var a in perf.Albums)
                    if (a.ARate.HasValue)
                    {
                        ratedCount++;
                        rate += a.ARate.Value;
                    }

                if (ratedCount == 0)
                    return @"Images\star_empty.png";
                else
                    return getStar((byte)(rate / ratedCount), (int)parameter);
            }

            Albums alb = value as Albums;

            if (alb == null)
                return null;

            if (alb.ARate.HasValue)
                return getStar(alb.ARate.Value, (int)parameter);
            else
                return @"Images\star_empty.png";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
