using System;
using System.Globalization;
using System.Windows.Data;

namespace MusCatalog.Converters
{
    /// <summary>
    /// Converter:      Performer  =>  Brief signature indicating the number of performer's albums
    /// </summary>
    class AlbumCountConverter:  IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            int albumCount = (int)value;

            switch (albumCount)
            {
                case 0: return "No albums";
                case 1: return "1 album";
                default: return albumCount + " albums";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
