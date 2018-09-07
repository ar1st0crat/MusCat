using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MusCat.Converters
{
    /// <summary>
    /// Converter:  Album image path   =>  (optionally reduced) image with album photo or default photo
    /// </summary>
    public class AlbumImageConverter : IValueConverter
    {
        private const string NoAlbumImage = @"../Images/vinyl_blue.png";

        /// <summary>
        /// Basic conversion
        /// </summary>
        /// <param name="parameter">The height of a thumbnail image, or null if the original image should be returned</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var albumImagePath = value as string;

            if (string.IsNullOrEmpty(albumImagePath))
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
