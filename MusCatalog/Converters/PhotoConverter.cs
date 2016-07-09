using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MusCatalog.Model;
using MusCatalog.Utils;

namespace MusCatalog.Converters
{
    /// <summary>
    /// Converter:          Performer   =>  (optionally reduced) image with performer's photo or default photo
    ///                     Album       =>  (optionally reduced) image with album cover or default photo
    /// </summary>
    public class PhotoConverter: IValueConverter
    {
        private const string NoPerformerPhoto = @"../Images/no_photo.png";
        private const string NoAlbumPhoto = @"../Images/vinyl_blue.png";

        /// <summary>
        /// Basic convertion
        /// </summary>
        /// <param name="parameter">The height of a thumbnail image, or null if the original image should be returned</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // if the current converter is yielding a performer photo path
            Performer perf = value as Performer;

            if (perf != null)
            {
                string performerPhotoPath = FileLocator.GetPathImagePerformer(perf);

                // if the photo exists
                if (performerPhotoPath != "")
                {
                    // optimization: reduce original image (set DecodePixelHeight property)
                    // and release the original image by returning new WriteableBitmap
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    if (parameter != null)
                    {
                        bi.DecodePixelHeight = (int)(double)parameter;
                    }
                    bi.UriSource = new Uri(performerPhotoPath);
                    bi.EndInit();
                    
                    return new WriteableBitmap(bi);
                }

                return NoPerformerPhoto;
            }

            // or maybe the converter's yielding an album photo path
            Album alb = value as Album;

            if (alb != null)
            {
                string albumPhotoPath = FileLocator.GetPathImageAlbum(alb);

                if (albumPhotoPath != "")
                {
                    // optimization is the same as for performer photo image
                    BitmapImage bi = new BitmapImage();
                    bi.BeginInit();
                    bi.CacheOption = BitmapCacheOption.OnLoad;
                    bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    if (parameter != null)
                    {
                        bi.DecodePixelHeight = (int)(double)parameter;
                    }
                    bi.UriSource = new Uri(albumPhotoPath);
                    bi.EndInit();

                    return new WriteableBitmap(bi);
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
