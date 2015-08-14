using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;


namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for AlbumWindow.xaml
    /// Some WinForms programming style here, since interaction between MediaPlayer and Slider could be made only in code-behind
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

        // Audio player
        AudioPlayer player = new AudioPlayer();
        
        // playback timer to synchronize playback slider with MediaPlayer
        DispatcherTimer playbackTimer = new DispatcherTimer();

        // slider and image references in selected items
        Slider curSlider = null;
        Image curPlaybackImage = null;

        // boolean flag indicating that items in songlist are selected by user
        bool bSelectionByUser = false;


        /// <summary>
        /// Window constructor sets up: 1) DispatcherTimer, 2) itemsource for a songlist
        /// </summary>
        /// <param name="a">Album whose info is shown in the window</param>
        public AlbumWindow(Album a)
        {
            InitializeComponent();

            // setting up timer for songs playback
            playbackTimer.Tick += new EventHandler( PlaybackTimerTick );
            playbackTimer.Interval = TimeSpan.FromSeconds(2);

            // save current album in 'album' variable
            album = a;

            // load and prepare all songs from the album for further actions
            using (var context = new MusCatEntities())
            {
                albumSongs = context.Songs.Where(s => s.Album.ID == a.ID).ToList();

                foreach (var song in albumSongs)
                {
                    song.Album = album;
                }

                this.rateAlbum.DataContext = a;
                this.AlbumInfoPanel.DataContext = a;
                this.songlist.ItemsSource = albumSongs;
            }
        }

        
        /// <summary>
        /// Freeze media player when the window is closing to avoid a memory leak
        /// </summary>
        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            player.Freeze();
        }

        /// <summary>
        /// The purpose of this little handler is to store the reference to playback slider and playback button image
        /// (we'll easily reference the playback slider without needing to traverse across the tree of controls)
        /// </summary>
        private void SliderLoaded(object sender, RoutedEventArgs e)
        {
            DockPanel panel = sender as DockPanel;
            curSlider = panel.FindName("PlaybackSlider") as Slider;
            curPlaybackImage = panel.FindName("PlayButtonImage") as Image;
        }

        /// <summary>
        /// After each click on the songlist item, set the indicator (that items in songlist are selected by user) to true
        /// </summary>
        private void SonglistMouseDown(object sender, MouseButtonEventArgs e)
        {
            bSelectionByUser = true;
        }

        ///
        /// ============================== Song playback functions ==================================
        ///
        private void PlaySong()
        {
            var song = this.songlist.SelectedItem as Song;
            string songfile = FileLocator.FindSongPath(albumSongs[Convert.ToInt32(song.TrackNo) - 1]);

            try
            {
                player.Play(songfile, SongPlaybackStopped);
                playbackTimer.Start();
                curPlaybackImage.Source = imagePause;
            }
            catch (Exception)
            {
                MessageBox.Show("Sorry, song could not be played");
            }
        }

        private void PauseSong()
        {
            player.Pause();
            playbackTimer.Stop();
            curPlaybackImage.Source = imagePlay;
        }

        private void ResumeSong()
        {
            player.Resume();
            playbackTimer.Start();
            curPlaybackImage.Source = imagePause;
        }

        private void StopSong()
        {
            player.Stop();
            playbackTimer.Stop();
            curPlaybackImage.Source = imagePlay;
        }
        /// =========================================================================================
        
        /// <summary>
        /// Start or stop playing the selected song (playback button click handler)
        /// </summary>
        private void PlaySongClick(object sender, RoutedEventArgs e)
        {
            switch (player.SongPlaybackState)
            {
                case PlaybackState.PLAY:
                    PauseSong();
                    break;
                case PlaybackState.PAUSE:
                    ResumeSong();
                    break;
                case PlaybackState.STOP:
                    PlaySong();
                    break;
            }
        }
        
        /// <summary>
        /// Handler of an event fired when the song reached the end
        /// </summary>
        private void SongPlaybackStopped(object sender, EventArgs e)
        {
            StopSong();

            // play next song
            if (this.songlist.SelectedIndex == this.songlist.Items.Count - 1)
            {
                return;
            }
            this.songlist.SelectedIndex++;
            this.songlist.Focus();
        }


        /// <summary>
        /// Rewind the song to the position specified by playback slider
        /// </summary>
        /// <param name="sender">playback slider</param>
        private void SeekPlaybackPosition(object sender, DragCompletedEventArgs e)  //RoutedPropertyChangedEventArgs<double> e)
        {
            if (player.SongPlaybackState == PlaybackState.STOP)
            {
                return;
            }

            player.Seek( ((Slider)sender).Value / 10.0 );
        }

        
        /// <summary>
        /// Update the slider thumb position according to current position of MediaPlayer
        /// </summary>
        private void PlaybackTimerTick(object sender, EventArgs e)
        {
            if (player.SongPlaybackState != PlaybackState.PLAY || curSlider == null)
            {
                return;
            }

            curSlider.Value = player.TimePercent() * 10.0;
        }


        private void SelectedSongChanged(object sender, SelectionChangedEventArgs e)
        {
            if ( !bSelectionByUser )
            {
                return;
            }

            if ( player.SongPlaybackState != PlaybackState.STOP )
            {
                StopSong();
            }

            PlaySong();
        }



        // ======================== MOVE TO EditAlbumWindow.xaml.cs ==========================

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
            for (int i = 0; i < starPos - 1; i++)
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
            for (int i = starPos; i < 5; i++)
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

            if (album.Rate.HasValue)
            {
                int i = 0;
                for (; i < album.Rate / 2; i++)
                {
                    ((Image)this.rateAlbum.Children[i]).Source = imageStar;
                }

                if (album.Rate.Value % 2 == 1)
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
            using (var context = new MusCatEntities())
            {
                context.Entry(album).State = System.Data.EntityState.Modified;
                context.SaveChanges();
            }
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
                var filepaths = FileLocator.MakePathImageAlbum( album );
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
