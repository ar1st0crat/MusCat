using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusCatalog
{
    /// <summary>
    /// Interaction logic for AlbumWindow.xaml
    /// </summary>
    public partial class AlbumWindow : Window
    {
        Albums album;

        public AlbumWindow( Albums a )
        {
            InitializeComponent();

            album = a;

            using ( var context = new MusCatEntities() )
            {
                this.rateAlbum.DataContext = a;
                AlbumInfoPanel.DataContext = a;
                this.songlist.ItemsSource = context.Songs.Where(s => s.Albums.AID == a.AID).ToList();
            }
        }

        private void EditAlbumButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show( "OK!" );
        }




        bool bPlaying = false;
        
        // bitmaps for playback buttons
        BitmapImage imagePlay = new BitmapImage(new Uri(@"Images\play.png", UriKind.Relative));
        BitmapImage imagePause = new BitmapImage(new Uri(@"Images\pause.png", UriKind.Relative));

        BitmapImage imageStar = new BitmapImage(new Uri(@"Images\star.png", UriKind.Relative));
        BitmapImage imageHalfStar = new BitmapImage(new Uri(@"Images\star_half.png", UriKind.Relative));
        BitmapImage imageEmptyStar = new BitmapImage(new Uri(@"Images\star_empty.png", UriKind.Relative));


        byte starPos = 0;


        private void PlaySongClick(object sender, RoutedEventArgs e)
        {
            if (!bPlaying)
            {
                bPlaying = true;
                ((Image)((Button)sender).FindName( "playButton" )).Source = imagePause;
            }
            else
            {
                bPlaying = false;
                ((Image)((Button)sender).FindName("playButton")).Source = imagePlay;
            }
        }

        private void StarMouseMove(object sender, MouseEventArgs e)
        {
            starPos = 0;

            foreach (var elem in this.rateAlbum.Children)
            {
                starPos++;

                if (elem == (Image)sender)
                    break;
            }

            for (int i = 0; i < starPos-1; i++ )
            {
                ((Image)this.rateAlbum.Children[i]).Source = imageStar;
            }

            if (e.GetPosition((Image)sender).X > 25 / 2)
                ((Image)sender).Source = imageStar;
            else
                ((Image)sender).Source = imageHalfStar;

            for (int i = starPos; i < 5; i++ )
            {
                ((Image)this.rateAlbum.Children[i]).Source = imageEmptyStar;
            }
        }

        private void StarMouseLeave(object sender, MouseEventArgs e)
        {
            if (!album.ARate.HasValue || album.ARate > 0)
                return;

            starPos = 0;
            for (int i = 0; i < 5; i++)
            {
                ((Image)this.rateAlbum.Children[i]).Source = imageEmptyStar;
            }
        }

        private void StarMouseDown(object sender, MouseButtonEventArgs e)
        {
            using ( var context = new MusCatEntities() )
            {
                album.ARate = (byte)(starPos * 2);

                if ( ((Image)sender).Source == imageHalfStar )
                {
                    album.ARate--;
                }
                context.Entry(album).State = System.Data.EntityState.Modified;
                context.SaveChanges( );
            }
        }
    }
}
