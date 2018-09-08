using System;
using System.Globalization;
using System.Windows.Data;

namespace MusCat.Converters
{
    /// <summary>
    /// Converter:      Performer  =>  Brief signature indicating the number of performer's albums
    /// </summary>
    public class TruncateTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value as string;

            if (text == null)
            {
                return null;
            }

            var truncatePos = (int)parameter;

            if (text.Length <= truncatePos)
            {
                return text;
            }
            
            return text.Substring(0, truncatePos)
                       .Insert(truncatePos, "...")
                       .Replace("\n", "");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
