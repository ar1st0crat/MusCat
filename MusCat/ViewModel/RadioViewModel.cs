using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
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
        public ObservableCollection<Song> RadioArchive => _radio.SongArchive;
        public ObservableCollection<Song> RadioUpcoming => _radio.UpcomingSongs;

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

            ChangeSongCommand = new RelayCommand(id =>
            {
                _radio.ChangeSong((long)id);
                UpdateSongs();
            });

            RemoveSongCommand = new RelayCommand(id =>
            {
                _radio.RemoveSong((long)id);
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
            
            _radio.MakeSonglist();

            PlayCurrentSong();
        }

        /// <summary>
        /// The method updates three properties (previous, next and currently played songs)
        /// </summary>
        private void UpdateSongs()
        {
            RaisePropertyChanged("PreviousSong");
            RaisePropertyChanged("CurrentSong");
            RaisePropertyChanged("NextSong");
        }

        private void PlayNextSong()
        {
            _radio.MoveToNextSong();
            PlayCurrentSong();
        }

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

            albumView.LoadSongs();

            var albumWindow = new AlbumWindow
            {
                DataContext = new AlbumPlaybackViewModel(albumView)
            };

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
