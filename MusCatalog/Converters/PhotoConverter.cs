using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Data;
using System.Linq;
using System.Windows.Media.Imaging;

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
            // if the current converter is yielding a performer photo path
            Performer perf = value as Performer;

            if (perf != null)
            {
                string performerPhotoPath = MusCatFileLocator.GetPathImagePerformer(perf);

                if ( performerPhotoPath != "")
                {
                    return BitmapFrame.Create( new Uri( performerPhotoPath ),
                                                            BitmapCreateOptions.IgnoreImageCache,
                                                            BitmapCacheOption.OnLoad);
                }
                else
                {
                    return NoPerformerPhoto;
                }
            }

            // or maybe the converter's yielding an album photo path
            Album alb = value as Album;

            if (alb != null)
            {
                string albumPhotoPath = MusCatFileLocator.GetPathImageAlbum(alb);

                if (albumPhotoPath != "")
                {
                    return BitmapFrame.Create(new Uri(albumPhotoPath),
                                                            BitmapCreateOptions.IgnoreImageCache,
                                                            BitmapCacheOption.OnLoad);
                }
                else
                {
                    return NoAlbumPhoto;
                }
            }
            
            return "";
        }


        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
