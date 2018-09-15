using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Audio;
using MusCat.Core.Interfaces.Domain;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.Util;
using MusCat.ViewModels.Entities;

namespace MusCat.ViewModels
{
    class AlbumPlaybackViewModel : ViewModelBase
    {
        private readonly IAlbumService _albumService;
        
        public AlbumViewModel Album { get; set; }

        private ObservableCollection<Song> _songs;
        public ObservableCollection<Song> Songs
        {
            get { return _songs; }
            set
            {
                _songs = value;
                RaisePropertyChanged();
            }
        }
        
        private Song _selectedSong;
        public Song SelectedSong
        {
            get { return _selectedSong; }
            set
            {
                _selectedSong = value;
                RaisePropertyChanged();
                if (!_isLoading) PlaySong();
            }
        }

        private string _timePlayed;
        public string TimePlayed
        {
            get { return _timePlayed; }
            set
            {
                _timePlayed = value;
                RaisePropertyChanged();
            }
        }
        
        /// <summary>
        /// Album header to be displayed in the window title
        /// </summary>
        public string AlbumHeader => $"{Album.Performer?.Name} - {Album.Name} ({Album.ReleaseYear})";

        // Bitmaps for playback buttons

        private static readonly BitmapImage ImagePlay = App.Current.TryFindResource("ImagePlayButton") as BitmapImage;
        private static readonly BitmapImage ImagePause = App.Current.TryFindResource("ImagePauseButton") as BitmapImage;

        private BitmapImage _playbackImage = ImagePause;
        public BitmapImage PlaybackImage
        {
            get { return _playbackImage; }
            set
            {
                _playbackImage = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Audio player
        /// </summary>
        private readonly IAudioPlayer _player;
        private bool _isStopped;

        /// <summary>
        /// Song time percentage
        /// </summary>
        private double _playbackPercentage;
        public double PlaybackPercentage
        {
            get { return _playbackPercentage; }
            set
            {
                _playbackPercentage = value;
                TimePlayed = $"{(int)(_player.PlayedTime / 60)}:{_player.PlayedTime % 60:00}";
                RaisePropertyChanged();
            }
        }

        // Commands

        public RelayCommand WindowClosingCommand { get; private set; }
        public RelayCommand PlaybackCommand { get; private set; }
        public RelayCommand SeekPlaybackPositionCommand { get; private set; }
        public RelayCommand StartDragCommand { get; private set; }
        public RelayCommand StopDragCommand { get; private set; }
        public RelayCommand UpdateRateCommand { get; private set; }

        // This variable is set to true while the slider thumb is being dragged
        // (Binding the event "Thumb.DragCompleted" in XAML to any command 
        //  doesn't work for some reason ¯\_(ツ)_/¯)
        private bool _isDragged;

        private bool _isLoading;

        // optional reference to Performer's view (for UI update)
        public PerformerViewModel Performer { get; set; }


        public AlbumPlaybackViewModel(IAlbumService albumService, IAudioPlayer player)
        {
            Guard.AgainstNull(albumService);
            Guard.AgainstNull(player);

            _albumService = albumService;
            _player = player;
            
            // setting up commands
            PlaybackCommand = new RelayCommand(PlaybackSongAction);
            UpdateRateCommand = new RelayCommand(UpdateRate);
            SeekPlaybackPositionCommand = new RelayCommand(SeekPlaybackPosition);

            // StopAndDispose media player when the window is closing to avoid a memory leak
            WindowClosingCommand = new RelayCommand(() =>
            {
                _isStopped = true;
                _player.Close();
            });
            
            // toggle the _isDragged variable
            StartDragCommand = new RelayCommand(() => _isDragged = true);             
            StopDragCommand = new RelayCommand(() => _isDragged = false);
        }
        
        public async Task LoadSongsAsync()
        {
            _isLoading = true;

            Songs = new ObservableCollection<Song>(
                await _albumService.LoadAlbumSongsAsync(Album.Id));

            _isLoading = false;
            SelectedSong = null;

            StartPlayer();

            // but don't play anything until user clicks the song
            _player.Stop();         
        }

        /// <summary>
        /// There's one general task associated with the album window
        /// whose purpose is to play selected song in the background thread
        /// </summary>
        private void StartPlayer()
        {
            Task.Run(async () =>
            {
                while (!_isStopped)
                {
                    await Task.Delay(1000);

                    if (SelectedSong != null)
                    {
                        UpdateSong();
                    }
                }
            });
        }

        private void UpdateSong()
        {
            if (_player.IsStopped && !_player.IsStoppedManually)
            {
                if (SelectedSong != Songs.Last())
                {
                    SelectedSong = Songs.SkipWhile(s => s != SelectedSong)
                                        .Skip(1)
                                        .FirstOrDefault();
                    PlaySong();
                }
                else
                {
                    SelectedSong = null;
                    PlaybackImage = ImagePlay;
                }
            }

            PlaybackPercentage = _player.PlayedTimePercent * 10.0;
        }

        #region Song playback functions

        private void PlaySong()
        {
            if (SelectedSong == null)
            {
                PlaybackImage = ImagePlay;
                return;
            }

            if (_player.SongPlaybackState != PlaybackState.Stop)
            {
                _player.Stop();
            }

            var songfile = FileLocator.FindSongPath(SelectedSong);

            PlaybackPercentage = 0.0;

            try
            {
                _player.Play(songfile);
                PlaybackImage = ImagePause;
            }
            catch (Exception)
            {
                // if the exception was thrown, show message
                // and manually set STOP flag in order to not proceed to next song
                _player.IsStoppedManually = true;
                MessageBox.Show("Sorry, song could not be played");
            }
        }

        /// <summary>
        /// Playback actions: Play / Pause
        /// </summary>
        private void PlaybackSongAction()
        {
            switch (_player.SongPlaybackState)
            {
                case PlaybackState.Play:
                    _player.Pause();
                    PlaybackImage = ImagePlay;
                    break;
                case PlaybackState.Pause:
                    _player.Resume();
                    PlaybackImage = ImagePause;
                    break;
            }
        }

        /// <summary>
        /// Rewind the song to the position specified by PlaybackPercentage
        /// (property bound to playback slider's value)
        /// </summary>
        private void SeekPlaybackPosition()
        {
            // if the slider value was changed with timer (not by user) 
            // or the song is stopped
            if (!_isDragged || _player.SongPlaybackState == PlaybackState.Stop)
            {
                // then do nothing
                return;
            }

            _player.Seek(PlaybackPercentage / 10.0);
        }

        #endregion

        /// <summary>
        /// Just an additional feature of the AlbumWindow:
        /// user can't edit album info except that he/she can update album rate
        /// by clicking on the 5-star rate control
        /// </summary>
        private void UpdateRate()
        {
            _albumService.UpdateAlbumRate(Album.Id, Album.Rate);
            Performer?.UpdateAlbumCollectionRate();
        }
    }
}
