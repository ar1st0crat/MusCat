using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using MusCat.Entities;
using MusCat.Services;

namespace MusCat.Converters
{
    /// <summary>
    /// Converter:          Performer   =>  (optionally reduced) image with performer's photo or default photo
    ///                     Album       =>  (optionally reduced) image with album cover or default photo
    /// </summary>
    public class PhotoConverter : IValueConverter
    {
        private const string NoPerformerImage = @"../Images/no_photo.png";
        private const string NoAlbumImage = @"../Images/vinyl_blue.png";

        /// <summary>
        /// Basic convertion
        /// </summary>
        /// <param name="parameter">The height of a thumbnail image, or null if the original image should be returned</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // if the current converter is yielding a performer photo path
            var performer = value as Performer;

            if (performer != null)
            {
                var performerImagePath = FileLocator.GetPerformerImagePath(performer);

                if (performerImagePath == "")
                {
                    return NoPerformerImage;
                }

                // optimization: reduce original image (set DecodePixelHeight property)
                // and release the original image by returning new WriteableBitmap
                var performerImage = new BitmapImage();
                performerImage.BeginInit();
                performerImage.CacheOption = BitmapCacheOption.OnLoad;
                performerImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                if (parameter != null)
                {
                    performerImage.DecodePixelHeight = (int)(double)parameter;
                }
                performerImage.UriSource = new Uri(performerImagePath);
                performerImage.EndInit();
                performerImage.Freeze();
                    
                return new WriteableBitmap(performerImage);
            }

            // or maybe the converter's yielding an album photo path
            var album = value as Album;

            if (album == null)
            {
                return "";
            }

            var albumImagePath = FileLocator.GetAlbumImagePath(album);

            if (albumImagePath == "")
            {
                return NoAlbumImage;
            }

            // optimization is the same as for performer photo image
            var albumImage = new BitmapImage();
            albumImage.BeginInit();
            albumImage.CacheOption = BitmapCacheOption.OnLoad;
            albumImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            if (parameter != null)
            {
                albumImage.DecodePixelHeight = (int)(double)parameter;
            }
            albumImage.UriSource = new Uri(albumImagePath);
            albumImage.EndInit();
            albumImage.Freeze();

            return new WriteableBitmap(albumImage);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
