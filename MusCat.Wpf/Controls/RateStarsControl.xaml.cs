using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MusCat.Controls
{
    /// <summary>
    /// Interaction logic for RateStarsControl.xaml.
    /// RateStarsControl is a clickable 5-star rating panel.
    /// </summary>
    public partial class RateStarsControl : UserControl
    {
        // add property called "Rate"
        public static readonly DependencyProperty RateProperty = 
            DependencyProperty.Register("Rate", typeof(byte?), typeof(RateStarsControl),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, RateChanged));

        // bitmaps for stars
        private static readonly BitmapImage ImageStar = App.Current.TryFindResource("ImageStar") as BitmapImage;
        private static readonly BitmapImage ImageHalfStar = App.Current.TryFindResource("ImageHalfStar") as BitmapImage;
        private static readonly BitmapImage ImageEmptyStar = App.Current.TryFindResource("ImageEmptyStar") as BitmapImage;

        // rating parameters
        private const byte MaxRate = 10;
        private const int StarsCount = 5;

        public RateStarsControl()
        {
            InitializeComponent();
        }

        public byte? Rate
        {
            get
            {
                return (byte?)GetValue(RateProperty);
            }
            set
            {
                if (value <= MaxRate || value == null)
                {
                    SetValue(RateProperty, value);
                }
                else
                {
                    SetValue(RateProperty, MaxRate);
                }
            }
        }

        private static void RateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var item = sender as RateStarsControl;
            var children = ((Grid)(item.Content)).Children;

            DrawStars((byte?)e.NewValue, children);
        }

        private void StarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }

            var star = sender as Image;

            var newRate = int.Parse(star.Tag.ToString()) * 2;

            if (star.Source == ImageHalfStar)
            {
                newRate--;
            }

            // save the clicked rate
            Rate = (byte?)newRate;
        }

        private void ResetRate(object sender, MouseButtonEventArgs e)
        {
            Rate = null;
        }

        private void StarMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }

            var star = sender as Image;

            // calculate rate depending on the mouse coordinates
            byte? rate = byte.Parse(star.Tag.ToString());
            rate *= 2;

            if (e.GetPosition(star).X < (Width / StarsCount) / 2)
            {
                rate--;
            }

            DrawStars(rate, StarGrid.Children);
        }

        private void StarMouseLeave(object sender, MouseEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }

            // just draw stars according to the current rate (before user changed it by clicking)
            DrawStars(Rate, StarGrid.Children);
        }

        private static void DrawStars(byte? rate, UIElementCollection stars)
        {
            // if rate is null, then simply draw empty stars
            if (!rate.HasValue || rate == 0)
            {
                for (var i = 0; i < StarsCount; i++)
                {
                    ((Image)stars[i]).Source = ImageEmptyStar;
                }
                return;
            }

            // otherwise, calculate the "middle" star position that divides full stars and empty stars
            var starPos = (byte)((rate - 1) / 2);

            // draw all stars to the left as "full" stars
            for (var i = 0; i < starPos; i++)
            {
                ((Image)stars[i]).Source = ImageStar;
            }

            // the middle star can be half or empty depending of rate parity
            ((Image)stars[starPos]).Source = rate % 2 == 1 ? ImageHalfStar : ImageStar;

            // rest of the stars are empty
            for (var i = starPos + 1; i < StarsCount; i++)
            {
                ((Image)stars[i]).Source = ImageEmptyStar;
            }
        }
    }
}
