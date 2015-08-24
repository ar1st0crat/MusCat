using MusCatalog.ViewModel;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace MusCatalog.View
{
    /// <summary>
    /// Class for interaction logic for RadioPlayerWindow.xaml
    /// </summary>
    public partial class RadioPlayerWindow : Window
    {
        // Bitmaps for playback buttons
        BitmapImage imagePlay = App.Current.Resources[ "ImagePlayButton" ] as BitmapImage;
        BitmapImage imagePause = App.Current.Resources[ "ImagePauseButton" ] as BitmapImage;

        // Radio Station
        Radio radio = new Radio();

        
        public RadioPlayerWindow()
        {
            InitializeComponent();

            // we add two songs to the playlist right away:
            // 1. The song for current playback
            radio.AddSong();
            // 2. The upcoming song
            radio.AddSong();
            
            // Update MVVM GUI song elements
            UpdateGUISongs();

            // Start playing the first song right away
            StartPlayingCurrentSong();
        }


        /// <summary>
        /// The method correctly sets data contexts for song slots in the main window
        /// </summary>
        private void UpdateGUISongs()
        {
            this.prevSongPanel.DataContext = radio.PrevSong();
            this.curSongPanel.DataContext = radio.CurrentSong();
            this.nextSongPanel.DataContext = radio.NextSong();
            this.Archive.ItemsSource = radio.GetSongArchive();
			this.Archive.Items.Refresh();
        }


        /// <summary>
        /// Switch to next song in the playlist
        /// </summary>
        private void NextSong()
        {
            radio.MoveToNextSong();

            // update GUI
            UpdateGUISongs();

            // after we added new upcoming song we must play the current song
            // (this song has been "upcoming" before we added new song to the playlist)
            StartPlayingCurrentSong();
        }

        /// <summary>
        /// 
        /// </summary>
        private void PrevSong()
        {
            radio.MoveToPrevSong();

            // update GUI
            UpdateGUISongs();

            // after switching to previous song we play it right away
            StartPlayingCurrentSong();
        }

        /// <summary>
        /// 
        /// </summary>
        private void StartPlayingCurrentSong()
        {
            radio.PlayCurrentSong( SongPlaybackStopped );
            this.playback.Source = imagePause;
        }
        
        /// <summary>
        /// Handler of an event fired when the current song reached the end
        /// </summary>
        private void SongPlaybackStopped(object sender, EventArgs e)
        {
            // if the stopping of the song wasn't initiated by user,
            // then the current song is over and we switch to next song
            if (radio.Player.SongPlaybackState != PlaybackState.STOP)
            {
                NextSong();
            }
        }

        private void PausePlayMouseDown(object sender, MouseButtonEventArgs e)
        {
            switch (radio.Player.SongPlaybackState)
            {
                case PlaybackState.PLAY:
                    radio.Player.Pause();
                    this.playback.Source = imagePlay;
                    break;
                case PlaybackState.PAUSE:
                    radio.Player.Resume();
                    this.playback.Source = imagePause;
                    break;
                case PlaybackState.STOP:
                    StartPlayingCurrentSong();
                    this.playback.Source = imagePause;
                    break;
            }
        }
        
        private void StopMouseDown(object sender, MouseButtonEventArgs e)
        {
            radio.Player.Stop();
            this.playback.Source = imagePlay;
        }
        
        private void NextMouseDown(object sender, MouseButtonEventArgs e)
        {
            NextSong();
        }
        
        private void PrevMouseDown(object sender, MouseButtonEventArgs e)
        {
            PrevSong();
        }
        
        private void RadioPlayerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                NextSong();
            }
            else if (e.Key == Key.Left)
            {
                PrevSong();
            }
        }

        private void SliderVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            radio.Player.SetVolume((float)this.VolumeSlider.Value / 10.0f);
        }

        private void CurrentAlbumClick(object sender, MouseButtonEventArgs e)
        {
            var album = new AlbumViewModel { Album = radio.CurrentSong().Album };

            // lazy load songs of selected album
            album.LoadSongs();

            AlbumWindow albumWindow = new AlbumWindow();
            albumWindow.DataContext = new AlbumPlaybackViewModel( album );
            albumWindow.Show();
        }
    }
}
