using MusCatalog.Model;
using MusCatalog.Utils;
using MusCatalog.View;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
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
                radio.Player.SetVolume((float)value / 10.0f);
                RaisePropertyChanged("SongVolume");
            }
        }
        public Song PreviousSong { get; set; }
        public Song CurrentSong { get; set; }
        public Song NextSong { get; set; }
        public ObservableCollection<Song> RadioArchive
        {
            get { return radio.SongArchive; }
        }

        // commands
        public ICommand PlaybackCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PreviousSongCommand { get; private set; }
        public ICommand NextSongCommand { get; private set; }
        public ICommand ShowAlbumCommand { get; private set; }
        public ICommand WindowClosingCommand { get; private set; }

        #endregion


        public RadioViewModel()
        {
            // setting up all commands
            PlaybackCommand = new RelayCommand(SongPlaybackAction);
            StopCommand = new RelayCommand(Stop);
            PreviousSongCommand = new RelayCommand(PlayPreviousSong);
            NextSongCommand = new RelayCommand(PlayNextSong);
            ShowAlbumCommand = new RelayCommand(ViewAlbumContainingCurrentSong);
            WindowClosingCommand = new RelayCommand(Close);

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
        /// The method updates three properties (previous, next and currently played songs)
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
            // after we added new upcoming song we must play the current song
            // (this song was "upcoming" before we added new song to the playlist)
            PlayCurrentSong();
        }

        public void PlayPreviousSong()
        {
            radio.MoveToPrevSong();
            // after switching to previous song we play it right away
            PlayCurrentSong();
        }

        private void PlayCurrentSong()
        {
            if (radio.Player.SongPlaybackState != PlaybackState.STOP)
            {
                radio.Player.Stop();
            }

            // play song using BackgroundWorker
            var bw = new BackgroundWorker();

            bw.DoWork += (o, e) =>
            {
                radio.PlayCurrentSong();

                // loop while song was not stopped (naturally or manually)
                while (!radio.Player.IsStopped())
                {
                    Thread.Sleep(1000);
                }
            };

            // When the song was stopped (naturally or manually)
            bw.RunWorkerCompleted += (o, e) =>
            {
                if (!radio.Player.IsManualStop)
                {
                    // ...if naturally, then switch to next song in radio tracklist
                    PlayNextSong();
                }
            };

            bw.RunWorkerAsync();

            UpdateSongs();
            PlaybackImage = imagePause;
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
            albumWindow.DataContext = new AlbumPlaybackViewModel(albumView);
            albumWindow.Show();
        }

        /// <summary>
        /// StopAndDispose media player when the window is closing to avoid a memory leak
        /// </summary>
        public void Close()
        {
            radio.Player.StopAndDispose();
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
