using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for AlbumWindow.xaml
    /// </summary>
    public partial class AlbumWindow : Window
    {
        // Current album displayed in the window
        Album album;
        
        // Song list
        List<Song> albumSongs = new List<Song>();
                        
        // bitmaps for playback buttons
        BitmapImage imagePlay = App.Current.TryFindResource( "ImagePlayButton" ) as BitmapImage;
        BitmapImage imagePause = App.Current.TryFindResource( "ImagePauseButton" ) as BitmapImage;

        // bitmaps for stars
        BitmapImage imageStar = App.Current.TryFindResource( "ImageStar" ) as BitmapImage;
        BitmapImage imageHalfStar = App.Current.TryFindResource( "ImageHalfStar" ) as BitmapImage;
        BitmapImage imageEmptyStar = App.Current.TryFindResource( "ImageEmptyStar" ) as BitmapImage;

        // number of the star that was clicked
        byte starPos = 0;

        // indicator of album track playback
        bool bPlaying = false;

        // Audio player
        MusCatPlayer player = new MusCatPlayer();
        
        // playback timer
        DispatcherTimer playbackTimer = new DispatcherTimer();


        public AlbumWindow(Album a)
        {
            InitializeComponent();

            // setting up timer for songs playback
            playbackTimer.Tick += new EventHandler( PlaybackTimerTick );
            playbackTimer.Interval = new TimeSpan( 0, 0, 2 );

            //
            album = a;

            //
            using (var context = new MusCatEntities())
            {
                albumSongs = context.Songs.Where(s => s.Album.ID == a.ID).ToList();

                var AlbumID = a.ID;

                var curAlbum = (from albs in context.Albums
                                where albs.ID == AlbumID
                                select albs).First();

                var curPerformer = (from p in context.Performers
                                              where p.ID == curAlbum.Performer.ID
                                              select p).First();

                foreach (var song in albumSongs)
                {
                    // include the corresponding album of our song
                    song.Album = curAlbum;

                    // do the same thing with performer for included album
                    song.Album.Performer = curPerformer;
                }

                this.rateAlbum.DataContext = a;
                AlbumInfoPanel.DataContext = a;

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
                Button btn = sender as Button;
                
                string fileSong = MusCatFileLocator.FindSongPath(
                                            albumSongs[ Convert.ToInt32( btn.CommandParameter ) - 1 ] );
                try
                {
                    player.Play(fileSong, SongPlaybackStopped);

                    bPlaying = true;
                    playbackTimer.Start();

                    ((Image)((Button)sender).FindName("playButton")).Source = imagePause;
                }
                catch (Exception)
                {
                    MessageBox.Show( "Song could not be played" );
                }
            }
            else
            {
                bPlaying = false;
                playbackTimer.Stop();

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

            if ( album.Rate.HasValue )
            {
                int i = 0;
                for ( ; i < album.Rate / 2; i++)
                {
                    ((Image)this.rateAlbum.Children[i]).Source = imageStar;
                }

                if ( album.Rate.Value % 2 == 1 )
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
            album.Rate = (byte)(starPos * 2);

            if (((Image)sender).Source == imageHalfStar)
            {
                album.Rate--;
            }

            // update database
            using ( var context = new MusCatEntities() )
            {
                context.Entry(album).State = System.Data.EntityState.Modified;
                context.SaveChanges( );
            }
        }

        
        private void SeekPlaybackPosition(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.Seek( ((Slider)sender).Value / 10.0 );
        }

        
        // TODO:
        private void PlaybackTimerTick(object sender, EventArgs e)
        {
            //var template = this.songlist.Template;
            //var playbackSlider = (Slider)template.FindName("PlaybackSlider", this.songlist);

            //playbackSlider.SetValue( player. );
        }


        private void AlbumCoverMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                if (!Clipboard.ContainsImage())
                {
                    MessageBox.Show("No image in clipboard!");
                    return;
                }

                //string filepath = string.Format(@"F:\{0}\{1}\Picture\{2}.jpg", char.ToUpperInvariant(album.Performer.Name[0]), album.Performer.Name, album.ID);
                var filepaths = MusCatFileLocator.MakePathImageAlbum( album );
                string filepath = filepaths[0];

                if (filepaths.Count > 1)
                {
                    ChoiceWindow choice = new ChoiceWindow();
                    choice.SetChoiceList(filepaths);
                    choice.ShowDialog();

                    if (choice.ChoiceResult == "")
                    {
                        return;
                    }
                    filepath = choice.ChoiceResult;
                }
                
                Directory.CreateDirectory(Path.GetDirectoryName(filepath));

                var image = Clipboard.GetImage();
                try
                {
                    // first check if file already exists
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                    }

                    using (var fileStream = new FileStream(filepath, FileMode.CreateNew))
                    {
                        BitmapEncoder encoder = new JpegBitmapEncoder();
                        encoder.Frames.Add(BitmapFrame.Create(image));
                        encoder.Save(fileStream);

                        this.AlbumCover.Source = encoder.Frames[0];
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }
    }
}
