using MusCatalog.Model;
using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using System.Windows.Media.Imaging;


namespace MusCatalog
{
    /// <summary>
    /// Converter:      Performer and star position   =>  _ _ _ STAR _       ( position = 3, image = STAR )
    ///                 Album and star position       =>  _ _ HALFSTAR _ _   ( position = 2, image = HALFSTAR )
    /// </summary>
    public class RateStarsConverter: IValueConverter
    {
        private BitmapImage ImagePathStar = App.Current.TryFindResource("ImageStar") as BitmapImage;
        private BitmapImage ImagePathHalfStar = App.Current.TryFindResource("ImageHalfStar") as BitmapImage;
        private BitmapImage ImagePathEmptyStar = App.Current.TryFindResource("ImageEmptyStar") as BitmapImage;

        /// <summary>
        /// Method chooses the correct image for a star image at specified position
        /// </summary>
        /// <param name="rate">The rate of an album or a performer</param>
        /// <param name="nStar">Star position</param>
        /// <returns>The image (with star type) at specified position</returns>
        private BitmapImage getStar(int rate, int nStar)
        {
            if (rate >= 2 * nStar)
            {
                return ImagePathStar;
            }
            else if (rate < 2 * nStar && 2 * nStar - rate == 1)
            {
                return ImagePathHalfStar;
            }
            else
            {
                return ImagePathEmptyStar;
            }
        }


        /// <summary>
        /// Method returns the correct image for a star image at specified position
        /// </summary>
        /// <param name="parameter">Star position (integer)</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performers perf = value as Performers;

            if (perf != null)
            {
                if (perf.Albums.Count == 0)
                {
                    return ImagePathEmptyStar;
                }
                
                int ratedCount = perf.Albums.Count(t => t.ARate.HasValue);

                if (ratedCount == 0)
                {
                    return ImagePathEmptyStar;
                }

                int sumRate = perf.Albums.Sum(t =>
                {
                    if (t.ARate.HasValue)
                        return t.ARate.Value;
                    else
                        return 0;
                });

                if (ratedCount > 2)
                {
                    int minRate = perf.Albums.Min(r => r.ARate).Value;
                    int maxRate = perf.Albums.Max(r => r.ARate).Value;
                    sumRate -= (minRate + maxRate);
                    ratedCount -= 2;
                }

                int totalRate = (byte)Math.Round((double)sumRate / ratedCount, MidpointRounding.AwayFromZero);

                return getStar( totalRate, (int)parameter);
            }

            Albums alb = value as Albums;

            if (alb == null)
            {
                return null;
            }

            if (alb.ARate.HasValue)
            {
                return getStar(alb.ARate.Value, (int)parameter);
            }
            else
            {
                return ImagePathEmptyStar;
            }
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
