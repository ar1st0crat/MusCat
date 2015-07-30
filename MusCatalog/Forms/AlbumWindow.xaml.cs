using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace MusCatalog
{
    /// <summary>
    /// Interaction logic for AlbumWindow.xaml
    /// </summary>
    public partial class AlbumWindow : Window
    {
        // Current album displayed in the window
        Albums album;
        
        //
        List<Songs> albumSongs = new List<Songs>();

                
        // bitmaps for playback buttons
        BitmapImage imagePlay = new BitmapImage(new Uri(@"../Images/play.png", UriKind.Relative));
        BitmapImage imagePause = new BitmapImage(new Uri(@"../Images/pause.png", UriKind.Relative));

        // bitmaps for stars
        BitmapImage imageStar = new BitmapImage(new Uri(@"../Images/star.png", UriKind.Relative));
        BitmapImage imageHalfStar = new BitmapImage(new Uri(@"../Images/star_half.png", UriKind.Relative));
        BitmapImage imageEmptyStar = new BitmapImage(new Uri(@"../Images/star_empty.png", UriKind.Relative));

        // number of the star that was clicked
        byte starPos = 0;

        // indicator of album track playback
        bool bPlaying = false;

        //
        MusCatPlayer player = new MusCatPlayer();
        


        public AlbumWindow(Albums a)
        {
            InitializeComponent();

            album = a;

            using (var context = new MusCatEntities())
            {
                this.rateAlbum.DataContext = a;
                AlbumInfoPanel.DataContext = a;

                albumSongs = context.Songs.Where(s => s.Albums.AID == a.AID).ToList();

                var AlbumID = a.AID;// albumSongs.First().AID;

                var curAlbum = (from albs in context.Albums
                                where albs.AID == AlbumID
                                select albs).First(); 

                var curPerformer = (from p in context.Performers
                                              where p.PID == curAlbum.Performers.PID
                                              select p).First();

                foreach (var song in albumSongs)
                {
                    // include the corresponding album of our song
                    song.Albums = curAlbum;

                    // do the same thing with performer for included album
                    song.Albums.Performers = curPerformer;
                }

                this.songlist.ItemsSource = albumSongs;
            }
        }


        private void EditAlbumButtonClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("OK!");
        }


        private void PlaySongClick(object sender, RoutedEventArgs e)
        {
            if (!bPlaying)
            {
                bPlaying = true;

                ((Image)((Button)sender).FindName( "playButton" )).Source = imagePause;

                
                string fileSong = MusCatFileLocator.FindSongPath( (Songs)this.songlist.SelectedItem );

                
                try
                {
                    player.Play(fileSong, SongPlaybackStopped);
                }
                catch (Exception)
                {
                    MessageBox.Show( "Song could not be played" );
                }
            }
            else
            {
                bPlaying = false;
                ((Image)((Button)sender).FindName( "playButton" )).Source = imagePlay;

                player.Stop();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongPlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            if (player.SongPlaybackState != PlaybackState.STOP)
            {
                player.SongPlaybackState = PlaybackState.STOP;
            }
        }



        /// <summary>
        /// 
        /// </summary>
        private void StarMouseMove(object sender, MouseEventArgs e)
        {
            starPos = 0;

            // loop to find out what star has triggered the MouseMove event
            foreach (var elem in this.rateAlbum.Children)
            {
                starPos++;

                if (elem == (Image)sender)
                    break;
            }

            // draw all stars to the left as "full" stars
            for (int i = 0; i < starPos-1; i++ )
            {
                ((Image)this.rateAlbum.Children[i]).Source = imageStar;
            }

            // if the X coordinate of mouse position exceeds the half of a star size
            if (e.GetPosition((Image)sender).X > 25 / 2)
            {
                // then draw full star
                ((Image)sender).Source = imageStar;
            }
            else
            {
                // else draw half of the star
                ((Image)sender).Source = imageHalfStar;
            }

            // rest of the stars are empty
            for (int i = starPos; i < 5; i++ )
            {
                ((Image)this.rateAlbum.Children[i]).Source = imageEmptyStar;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void StarMouseLeave(object sender, MouseEventArgs e)
        {
            starPos = 0;
            for (int i = 0; i < 5; i++)
            {
                ((Image)this.rateAlbum.Children[i]).Source = imageEmptyStar;
            }

            if ( album.ARate.HasValue )
            {
                int i = 0;
                for ( ; i < album.ARate / 2; i++)
                {
                    ((Image)this.rateAlbum.Children[i]).Source = imageStar;
                }

                if ( album.ARate.Value % 2 == 1 )
                {
                    ((Image)this.rateAlbum.Children[i]).Source = imageHalfStar;
                }
            }
        }


        /// <summary>
        /// When the user clicks on a star, we update the album rate in database
        /// </summary>
        private void StarMouseDown(object sender, MouseButtonEventArgs e)
        {
            album.ARate = (byte)(starPos * 2);

            if (((Image)sender).Source == imageHalfStar)
            {
                album.ARate--;
            }

            // update database
            using ( var context = new MusCatEntities() )
            {
                context.Entry(album).State = System.Data.EntityState.Modified;
                context.SaveChanges( );
            }
        }
    }
}
