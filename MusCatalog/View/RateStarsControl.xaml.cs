using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for RateStarsControl.xaml.
    /// RateStarsControl is a clickable 5-star rating panel.
    /// </summary>
    public partial class RateStarsControl : UserControl
    {
        // add property called "Rate"
        public static readonly DependencyProperty RateProperty = DependencyProperty.Register("Rate", typeof(byte?), typeof(RateStarsControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, new PropertyChangedCallback(RateChanged)));

        // bitmaps for stars
        private static BitmapImage imageStar = App.Current.TryFindResource("ImageStar") as BitmapImage;
        private static BitmapImage imageHalfStar = App.Current.TryFindResource("ImageHalfStar") as BitmapImage;
        private static BitmapImage imageEmptyStar = App.Current.TryFindResource("ImageEmptyStar") as BitmapImage;

        // rating parameters
        private byte maxRate = 10;
        private const int STARS_COUNT = 5;

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
                if (value <= maxRate || value == null)
                {
                    SetValue(RateProperty, value);
                }
                else
                {
                    SetValue(RateProperty, maxRate);
                }
            }
        }

        private static void RateChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            RateStarsControl item = sender as RateStarsControl;
            UIElementCollection children = ((Grid)(item.Content)).Children;

            DrawStars( (byte?)e.NewValue, children );
        }

        private void StarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            Image star = sender as Image;

            int newRate = int.Parse(star.Tag.ToString()) * 2;
            if (star.Source == imageHalfStar)
            {
                newRate--;
            }

            // save the clicked rate
            Rate = (byte?)newRate;
        }

        private void StarMouseMove(object sender, MouseEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            // calculate rate depending on the mouse coordinates
            byte? rate = byte.Parse(((Image)sender).Tag.ToString());
            rate *= 2;

            if (e.GetPosition((Image)sender).X < (this.Width / STARS_COUNT) / 2)
            {
                rate--;
            }

            DrawStars( rate, this.StarGrid.Children );
        }

        private void StarMouseLeave(object sender, MouseEventArgs e)
        {
            if (!this.IsEnabled)
            {
                return;
            }

            // just draw stars according to the current rate (before user changed it by clicking)
            DrawStars(Rate, this.StarGrid.Children);
        }

        private static void DrawStars( byte? rate, UIElementCollection stars )
        {
            // if rate is null, then simply draw empty stars
            if (!rate.HasValue)
            {
                for (int i = 0; i < STARS_COUNT; i++)
                {
                    ((Image)stars[i]).Source = imageEmptyStar;
                }
                return;
            }

            // otherwise, calculate the "middle" star position that divides full stars and empty stars
            byte starPos = (byte)( (rate - 1) / 2);

            // draw all stars to the left as "full" stars
            for (int i = 0; i < starPos; i++)
            {
                ((Image)stars[i]).Source = imageStar;
            }

            // the middle star can be half or empty depending of rate parity
            if (rate % 2 == 1)
            {
                ((Image)stars[starPos]).Source = imageHalfStar;
            }
            else
            {
                ((Image)stars[starPos]).Source = imageStar;
            }

            // rest of the stars are empty
            for (int i = starPos + 1; i < STARS_COUNT; i++)
            {
                ((Image)stars[i]).Source = imageEmptyStar;
            }
        }
    }
}
