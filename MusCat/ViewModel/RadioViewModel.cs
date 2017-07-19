using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MusCat.Model;
using MusCat.View;
using MusCat.Utils;

namespace MusCat.ViewModel
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

        public Song PreviousSong => _radio.PrevSong;
        public Song CurrentSong => _radio.CurrentSong;
        public Song NextSong => _radio.NextSong;
        public ObservableCollection<Song> RadioArchive => new ObservableCollection<Song>(_radio.SongArchive);
        public ObservableCollection<Song> RadioUpcoming => new ObservableCollection<Song>(_radio.UpcomingSongs);

        // commands
        public ICommand PlaybackCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PreviousSongCommand { get; private set; }
        public ICommand NextSongCommand { get; private set; }
        public ICommand ChangeSongCommand { get; private set; }
        public ICommand RemoveSongCommand { get; private set; }
        public ICommand ShowAlbumCommand { get; private set; }
        public ICommand WindowClosingCommand { get; private set; }

        #endregion


        public RadioViewModel()
        {
            // ===================== setting up all commands ============================

            PlaybackCommand = new RelayCommand(SongPlaybackAction);
            PreviousSongCommand = new RelayCommand(PlayPreviousSong);
            NextSongCommand = new RelayCommand(PlayNextSong);
            ShowAlbumCommand = new RelayCommand(ViewAlbumContainingCurrentSong);

            ChangeSongCommand = new RelayCommand(async id =>
            {
                await _radio.ChangeSongAsync((long)id);
                UpdateSongs();
            });

            RemoveSongCommand = new RelayCommand(async id =>
            {
                await _radio.RemoveSongAsync((long)id);
                UpdateSongs();
            });

            StopCommand = new RelayCommand(() =>
            {
                _radio.Player.Stop();
                PlaybackImage = ImagePlay;
            });

            // StopAndDispose media player when the window is closing
            // to avoid a memory leak
            WindowClosingCommand = new RelayCommand(() =>
            {
                _radio.Player.StopAndDispose();
            });

            // ===========================================================================
            
            _radio.MakeSonglistAsync()
                  .ContinueWith(task =>
                  {
                      UpdateSongs();
                      PlayCurrentSong();
                  });
        }

        /// <summary>
        /// The method updates three properties (previous, next and currently played songs)
        /// </summary>
        private void UpdateSongs()
        {
            RaisePropertyChanged("PreviousSong");
            RaisePropertyChanged("CurrentSong");
            RaisePropertyChanged("NextSong");
            RaisePropertyChanged("RadioArchive");
            RaisePropertyChanged("RadioUpcoming");
        }

        /// <summary>
        /// Switching to next song is done asynchronously
        /// since it involves selecting new random song for a list of upcoming songs
        /// (which may take some time)
        /// </summary>
        private void PlayNextSong()
        {
            _radio.MoveToNextSongAsync()
                  .ContinueWith(task => PlayCurrentSong());
        }

        /// <summary>
        /// Switching to previous song is done synchronously
        /// since his operation is very cheap (just recombinate songs in collections)
        /// </summary>
        private void PlayPreviousSong()
        {
            _radio.MoveToPrevSong();
            PlayCurrentSong();
        }

        private void PlayCurrentSong()
        {
            if (_radio.Player.SongPlaybackState != PlaybackState.Stop)
            {
                _radio.Player.Stop();
            }

            var bw = new BackgroundWorker();

            bw.DoWork += (o, e) =>
            {
                _radio.StartPlaying();
                
                UpdateSongs();
                PlaybackImage = ImagePause;
                
                // loop while song was not stopped (naturally or manually)
                do
                {
                    Task.Delay(1000).Wait();
                }
                while (!_radio.Player.IsStopped());
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
        }
        
        private void SongPlaybackAction()
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

        private void ViewAlbumContainingCurrentSong()
        {
            var albumView = new AlbumViewModel
            {
                Album = _radio.CurrentSong.Album
            };

            Task.Run(() => albumView.LoadSongs()).ContinueWith(task =>
            {
                var albumWindow = new AlbumWindow
                {
                    DataContext = new AlbumPlaybackViewModel(albumView)
                };

                albumWindow.Show();
            }, 
            TaskScheduler.FromCurrentSynchronizationContext());
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
