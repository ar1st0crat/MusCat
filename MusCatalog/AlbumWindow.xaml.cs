using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public AlbumWindow( Albums a )
        {
            InitializeComponent();

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
    }
}
