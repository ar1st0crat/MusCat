using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace MusCat.Converters
{
    /// <summary>
    /// Converter:  Performer image path   =>  (optionally reduced) image with performer's photo or default photo
    /// </summary>
    public class PerformerImageConverter : IValueConverter
    {
        private const string NoPerformerImage = @"../Images/no_photo.png";

        /// <summary>
        /// Basic conversion
        /// </summary>
        /// <param name="parameter">The height of a thumbnail image, or null if the original image should be returned</param>
        /// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var performerImagePath = value as string;

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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
