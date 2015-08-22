using MusCatalog.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;


namespace MusCatalog
{
    /// <summary>
    /// Converter:      Performer   =>  string Performer rate (e.g. "8/10")
    ///                 Album       =>  string Album rate (e.g. "8/10")
    /// </summary>
    public class RateConverter: IValueConverter
    {
        /// <summary>
        /// Total rate of a performer is calculated based on the following statistics of album rates:
        /// 
        ///     if the number of albums is more than 2 then the worst rate and the best rate are discarded
        ///     and the total rate is an average of remaining rates
        ///     
        ///     otherwise - the total rate is an average of album rates
        ///     
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performer perf = value as Performer;

            // ================================================== the current object is PERFORMER
            if (perf != null)
            {
                if (perf.Albums.Count == 0)
                {
                    return "Not rated";
                }

                int ratedCount = perf.Albums.Count(t => t.Rate.HasValue);

                if (ratedCount == 0)
                {
                    return "Not rated";
                }

                int sumRate = perf.Albums.Sum(t =>
                {
                    if (t.Rate.HasValue)
                        return t.Rate.Value;
                    else
                        return 0;
                });

                if ( ratedCount > 2 )
                {
                    int minRate = perf.Albums.Min(r => r.Rate).Value;
                    int maxRate = perf.Albums.Max(r => r.Rate).Value;
                    sumRate -= (minRate + maxRate);
                    ratedCount -= 2;
                }
                
                int totalRate = (int)Math.Round( (double)sumRate / ratedCount, MidpointRounding.AwayFromZero );

                return totalRate;// +"/10";
            }


            // ================================================== the current object is ALBUM
            Album albs = value as Album;

            if (albs != null)
            {
                if (albs.Rate.HasValue)
                {
                    return albs.Rate + "/10";
                }
                else
                {
                    return "Not rated";
                }
            }

            // ================================================== the current object is  byte? rate
            byte? rate = value as Nullable<byte>;
            if (rate.HasValue)
            {
                return rate.Value + "/10";
            }

            return "Not Rated";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            //throw new NotImplementedException();
            return value;
        }
    }
}
