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
        private static readonly BitmapImage ImagePlay = App.Current.Resources["ImagePlayButton"] as BitmapImage;
        private static readonly BitmapImage ImagePause = App.Current.Resources["ImagePauseButton"] as BitmapImage;
        
        // Radio Station
        private readonly Radio _radio = new Radio();

        #region INPC properties

        private BitmapImage _playbackImage = ImagePause;
        public BitmapImage PlaybackImage
        {
            get { return _playbackImage; }
            set
            {
                _playbackImage = value;
                RaisePropertyChanged("PlaybackImage");
            }
        }
        private float _songVolume = 5.0f;
        public float SongVolume
        {
            get
            {
                return _songVolume;
            }
            set
            {
                _songVolume = value;
                _radio.Player.SetVolume(value / 10.0f);
                RaisePropertyChanged("SongVolume");
            }
        }
        public Song PreviousSong { get; set; }
        public Song CurrentSong { get; set; }
        public Song NextSong { get; set; }
        public ObservableCollection<Song> RadioArchive => _radio.SongArchive;

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
            _radio.AddSong();
            // 2. The upcoming song
            _radio.AddSong();
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
            PreviousSong = _radio.PrevSong();
            CurrentSong = _radio.CurrentSong();
            NextSong = _radio.NextSong();

            RaisePropertyChanged("PreviousSong");
            RaisePropertyChanged("CurrentSong");
            RaisePropertyChanged("NextSong");
        }

        public void PlayNextSong()
        {
            _radio.MoveToNextSong();
            // after we added new upcoming song we must play the current song
            // (this song was "upcoming" before we added new song to the playlist)
            PlayCurrentSong();
        }

        public void PlayPreviousSong()
        {
            _radio.MoveToPrevSong();
            // after switching to previous song we play it right away
            PlayCurrentSong();
        }

        private void PlayCurrentSong()
        {
            if (_radio.Player.SongPlaybackState != PlaybackState.Stop)
            {
                _radio.Player.Stop();
            }

            // play song using BackgroundWorker
            var bw = new BackgroundWorker();

            bw.DoWork += (o, e) =>
            {
                _radio.PlayCurrentSong();

                // loop while song was not stopped (naturally or manually)
                while (!_radio.Player.IsStopped())
                {
                    Thread.Sleep(1000);
                }
            };

            // When the song was stopped (naturally or manually)
            bw.RunWorkerCompleted += (o, e) =>
            {
                if (!_radio.Player.IsStoppedManually)
                {
                    // ...if naturally, then switch to next song in radio tracklist
                    PlayNextSong();
                }
            };

            bw.RunWorkerAsync();

            UpdateSongs();
            PlaybackImage = ImagePause;
        }
        
        public void SongPlaybackAction()
        {
            switch (_radio.Player.SongPlaybackState)
            {
                case PlaybackState.Play:
                    _radio.Player.Pause();
                    PlaybackImage = ImagePlay;
                    break;
                case PlaybackState.Pause:
                    _radio.Player.Resume();
                    PlaybackImage = ImagePause;
                    break;
                case PlaybackState.Stop:
                    PlayCurrentSong();
                    PlaybackImage = ImagePause;
                    break;
            }
        }

        public void Stop()
        {
            _radio.Player.Stop();
            PlaybackImage = ImagePlay;
        }

        public void ViewAlbumContainingCurrentSong()
        {
            var albumView = new AlbumViewModel
            {
                Album = _radio.CurrentSong().Album
            };

            // load songs of selected album lazily
            albumView.LoadSongs();

            var albumWindow = new AlbumWindow
            {
                DataContext = new AlbumPlaybackViewModel(albumView)
            };
            albumWindow.Show();
        }

        /// <summary>
        /// StopAndDispose media player when the window is closing to avoid a memory leak
        /// </summary>
        public void Close()
        {
            _radio.Player.StopAndDispose();
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
