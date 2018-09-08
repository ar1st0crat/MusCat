using System;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace MusCat.Converters
{
    /// <summary>
    /// Converter:      Rate (8)   =>  string rate ("8/10") 
    ///                 Rate(null) =>  string rate ("Not rated")
    /// </summary>
    public class RateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rate = value as byte?;

            if (rate.HasValue)
            {
                return rate.Value + "/10";
            }

            return "Not rated";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var rate = value.ToString();
            
            if (rate.Contains('/'))
            {
                rate = rate.Substring(0, rate.IndexOf('/'));
            }

            byte parsedRate;

            return byte.TryParse(rate, out parsedRate) ? parsedRate : 0;
        }
    }
}
