using MusCatalog.Model;
using MusCatalog.View;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media.Imaging;


namespace MusCatalog.ViewModel
{
    class RadioViewModel : INotifyPropertyChanged
    {
        // Bitmaps for playback buttons
        private static BitmapImage imagePlay = App.Current.Resources[ "ImagePlayButton" ] as BitmapImage;
        private static BitmapImage imagePause = App.Current.Resources[ "ImagePauseButton" ] as BitmapImage;
        
        // Radio Station
        private Radio radio = new Radio();

        #region INPC properties

        private BitmapImage playbackImage = imagePause;
        public BitmapImage PlaybackImage
        {
            get { return playbackImage; }
            set
            {
                playbackImage = value;
                RaisePropertyChanged("PlaybackImage");
            }
        }
        private float songVolume = 5.0f;
        public float SongVolume
        {
            get
            {
                return songVolume;
            }
            set
            {
                songVolume = value;
                radio.Player.SetVolume( (float)value / 10.0f );
                RaisePropertyChanged( "SongVolume" );
            }
        }
        public Song PreviousSong { get; set; }
        public Song CurrentSong { get; set; }
        public Song NextSong { get; set; }
        public ObservableCollection<Song> RadioArchive
        {
            get { return radio.SongArchive; }
        }

        #endregion

        public RadioViewModel()
        {
            // we add two songs to the playlist right away:
            // 1. The song for current playback
            radio.AddSong();
            // 2. The upcoming song
            radio.AddSong();
            // Update properties
            UpdateSongs();
            // Start playing the first song right away
            PlayCurrentSong();
        }

        /// <summary>
        /// The method updates three properties (previous, next and currently playing songs)
        /// </summary>
        private void UpdateSongs()
        {
            PreviousSong = radio.PrevSong();
            CurrentSong = radio.CurrentSong();
            NextSong = radio.NextSong();

            RaisePropertyChanged("PreviousSong");
            RaisePropertyChanged("CurrentSong");
            RaisePropertyChanged("NextSong");
        }

        public void PlayNextSong()
        {
            radio.MoveToNextSong();
            UpdateSongs();
            // after we added new upcoming song we must play the current song
            // (this song was "upcoming" before we added new song to the playlist)
            PlayCurrentSong();
        }

        public void PlayPreviousSong()
        {
            radio.MoveToPrevSong();
            UpdateSongs();
            // after switching to previous song we play it right away
            PlayCurrentSong();
        }

        private void PlayCurrentSong()
        {
            radio.PlayCurrentSong(SongPlaybackStopped);
            PlaybackImage = imagePause;
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
                PlayNextSong();
            }
        }

        public void SongPlaybackAction()
        {
            switch (radio.Player.SongPlaybackState)
            {
                case PlaybackState.PLAY:
                    radio.Player.Pause();
                    PlaybackImage = imagePlay;
                    break;
                case PlaybackState.PAUSE:
                    radio.Player.Resume();
                    PlaybackImage = imagePause;
                    break;
                case PlaybackState.STOP:
                    PlayCurrentSong();
                    PlaybackImage = imagePause;
                    break;
            }
        }
        
        public void Stop()
        {
            radio.Player.Stop();
            PlaybackImage = imagePlay;
        }

        public void ViewAlbumContainingCurrentSong()
        {
            var albumView = new AlbumViewModel { Album = radio.CurrentSong().Album };

            // lazy load songs of selected album
            albumView.LoadSongs();

            AlbumWindow albumWindow = new AlbumWindow();
            albumWindow.DataContext = new AlbumPlaybackViewModel( albumView );
            albumWindow.Show();
        }

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        #endregion
    }
}
