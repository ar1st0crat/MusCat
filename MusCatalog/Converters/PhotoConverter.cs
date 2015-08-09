using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Linq;

namespace MusCatalog
{
    /// <summary>
    /// Converter:          Performer   =>  image path with performer's photo or default photo
    ///                     Album       =>  image path with album cover or default photo
    /// </summary>
    public class PhotoConverter: IValueConverter
    {
        private const string NoPerformerPhoto = @"../Images/no_photo.png";
        private const string NoAlbumPhoto = @"../Images/vinyl_blue.png";

        /// <summary>
        /// Basic convertion
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Performer perf = value as Performer;

            if (perf != null)
            {
                Regex reg = new Regex( @"(?i)(ph|f)oto.(png|jpe?g|gif|bmp)" );

                foreach (var startingPath in MusCatFileLocator.pathlist )
                {
                    string path = startingPath +
                                    perf.Name[0] + 
                                    Path.DirectorySeparatorChar +
                                    perf.Name + @"\Picture";

                    if (Directory.Exists(path))
                    {
                        var files = Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly)
                                                .Where(filename => reg.IsMatch(filename))
                                                .ToList();
                        if (files.Count > 0)
                            return files[0];
                    }
                }

                return NoPerformerPhoto;
            }


            Album alb = value as Album;

            if (alb != null)
            {
                foreach (var startingPath in MusCatFileLocator.pathlist )
                {
                    string path = startingPath + 
                                    alb.Performer.Name[0] + 
                                    Path.DirectorySeparatorChar +
                                    alb.Performer.Name + 
                                    @"\Picture\" + 
                                    alb.ID + ".jpg";

                    if (File.Exists(path))
                        return path;
                }

                return NoAlbumPhoto;
            }
            
            return "";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
