using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MusCat.Entities;
using MusCat.Repositories;
using MusCat.Services;
using MusCat.Utils;
using MusCat.Views;

namespace MusCat.ViewModels
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
            get { return _songVolume; }
            set
            {
                _songVolume = value;
                _radio.SetVolume(value / 10.0f);
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
            ShowAlbumCommand = new RelayCommand(async() => await ViewAlbumContainingCurrentSong());

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
                _radio.StopPlaying();
                PlaybackImage = ImagePlay;
            });

            // Stop radio when the window is closing to avoid a memory leak
            // (it will call StopAndDispose() for media player)
            WindowClosingCommand = new RelayCommand(() =>
            {
                _radio.Stop();
            });

            // ===========================================================================

            _radio.Update = UpdateSongs;

            _radio.MakeSonglistAsync()
                  .ContinueWith(task =>
                  {
                      UpdateSongs();
                      _radio.Start();
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
            PlaybackImage = ImagePause;
        }

        /// <summary>
        /// Switching to next song is done asynchronously
        /// since it involves selecting new random song for a list of upcoming songs
        /// (which may take some time)
        /// </summary>
        private void PlayNextSong()
        {
            _radio.MoveToNextSongAsync().ContinueWith(task => UpdateSongs());
        }

        /// <summary>
        /// Switching to previous song is done synchronously
        /// since his operation is very cheap (just recombinate songs in collections)
        /// </summary>
        private void PlayPreviousSong()
        {
            _radio.MoveToPrevSong();
            UpdateSongs();
        }
        
        private void SongPlaybackAction()
        {
            switch (_radio.SongPlaybackState)
            {
                case PlaybackState.Play:
                    _radio.PausePlaying();
                    PlaybackImage = ImagePlay;
                    break;
                case PlaybackState.Pause:
                    _radio.ResumePlaying();
                    PlaybackImage = ImagePause;
                    break;
                case PlaybackState.Stop:
                    _radio.StartPlaying();
                    PlaybackImage = ImagePause;
                    break;
            }
        }

        /// <summary>
        /// Method opens Album window for displaying album cover and tracklist.
        /// Since user chooses this option not very often, 
        /// we instantiate album repository ad-hoc right in the body of the method.
        /// </summary>
        private async Task ViewAlbumContainingCurrentSong()
        {
            var albumView = new AlbumViewModel
            {
                Album = _radio.CurrentSong.Album
            };

            var repository = new AlbumRepository(new MusCatEntities());

            albumView.Songs = new ObservableCollection<Song>(
                await repository.GetAlbumSongsAsync(_radio.CurrentSong.Album));

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
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
