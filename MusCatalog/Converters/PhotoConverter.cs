using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Windows.Data;


namespace MusCatalog
{
    /// <summary>
    /// Converter:          Performer   =>  image path with performer's photo or default photo
    ///                     Album       =>  image path with album cover or default photo
    /// </summary>
    public class PhotoConverter: IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performers perf = value as Performers;

            if (perf != null)
            {
                foreach (var startingPath in MusCatFileLocator.pathlist )
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
                foreach (var startingPath in MusCatFileLocator.pathlist )
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
