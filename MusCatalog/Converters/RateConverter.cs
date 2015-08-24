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
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // =================================================== if the current object is ALBUM
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

            // ==================================== if the current object is simply Nullable<byte>
            byte? rate = value as Nullable<byte>;
            if (rate.HasValue)
            {
                return rate.Value + "/10";
            }

            return "Not rated";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = value.ToString();
            byte converted = 0;
            if (val.Contains('/'))
            {
                val = val.Substring( 0, val.IndexOf('/') );
            }

            if (byte.TryParse(val, out converted))
            {
                return converted;
            }
            else
            {
                return 0;
            }
        }
    }
}
