using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Data;


namespace MusCatalog
{
    public class PhotoConverter: IValueConverter
    {
        private List<string> paths = new List<string>();

        public PhotoConverter()
        {
            paths.Add( @"F:\" );
            paths.Add( @"G:\" );
            paths.Add( @"G:\Other\" );
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performers perf = value as Performers;

            if (perf != null)
            {
                foreach (var startingPath in paths)
                {
                    string path = startingPath +
                                    perf.Performer[0] + 
                                    Path.DirectorySeparatorChar +
                                    perf.Performer +
                                    @"\Picture\foto.jpg";
                    if (File.Exists(path))
                        return path;
                }

                return @"../Images/no-photo.jpg";
            }


            Albums alb = value as Albums;

            if (alb != null)
            {
                foreach (var startingPath in paths)
                {
                    string path = startingPath + 
                                    alb.Performers.Performer[0] + 
                                    Path.DirectorySeparatorChar +
                                    alb.Performers.Performer + 
                                    @"\Picture\" + 
                                    alb.AID + ".jpg";

                    if (File.Exists(path))
                        return path;
                }

                return @"../Images/vinyl_blue.png";
            }
            
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
