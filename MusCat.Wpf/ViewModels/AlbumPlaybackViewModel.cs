using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AutoMapper;
using MusCat.Application.Interfaces;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Interfaces.Audio;
using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.Util;
using MusCat.ViewModels.Entities;
using MusCat.Views;

namespace MusCat.ViewModels
{
    class AlbumPlaybackViewModel : ViewModelBase
    {
        private readonly IAlbumService _albumService;
        private readonly IRateCalculator _rateCalculator;
        private readonly ILyricsWebLoader _lyricsWebLoader;
        private readonly IVideoLinkWebLoader _videoLinkWebLoader;

        public AlbumViewModel Album { get; set; }

        private ObservableCollection<Song> _songs;
        public ObservableCollection<Song> Songs
        {
            get => _songs;
            set
            {
                _songs = value;
                RaisePropertyChanged();
            }
        }

        private Song _selectedSong;
        public Song SelectedSong
        {
            get => _selectedSong;
            set
            {
                _selectedSong = value;
                RaisePropertyChanged();
                
                IsLyricsVisible = Visibility.Collapsed;

                if (!_isLoading) PlaySong();
            }
        }

        private string _timePlayed;
        public string TimePlayed
        {
            get => _timePlayed;
            set
            {
                _timePlayed = value;
                RaisePropertyChanged();
            }
        }

        private string _lyrics;
        public string Lyrics
        {
            get => _lyrics;
            set
            {
                _lyrics = value;
                RaisePropertyChanged();
            }
        }
        private Visibility _isLyricsVisible = Visibility.Collapsed;
        public Visibility IsLyricsVisible
        {
            get => _isLyricsVisible;
            set
            {
                _isLyricsVisible = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Album header to be displayed in the window title
        /// </summary>
        public string AlbumHeader => $"{Album.Performer?.Name} - {Album.Name} ({Album.ReleaseYear})";

        // Playback button Segoe MDL symbols

        private static readonly string SymbolPlay = "\uE768";
        private static readonly string SymbolPause = "\uE769";

        private string _playbackSymbol = SymbolPause;
        public string PlaybackSymbol
        {
            get => _playbackSymbol;
            set
            {
                _playbackSymbol = value;
                RaisePropertyChanged();
            }
        }

        private double _windowOpacity = 0.25;
        public double WindowOpacity
        {
            get => _windowOpacity;
            set
            {
                _windowOpacity = value;
                RaisePropertyChanged();
            }
        }

        private Visibility _isTracklistVisible = Visibility.Visible;
        public Visibility IsTracklistVisible
        {
            get => _isTracklistVisible;
            set
            {
                _isTracklistVisible = value;
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
            get => _playbackPercentage;
            set
            {
                _playbackPercentage = value;
                RaisePropertyChanged();

                TimePlayed = $"{(int)_player.PlayedTime / 60}:{(int)_player.PlayedTime % 60:00}";
            }
        }

        private float _songVolume = 5.0f;
        public float SongVolume
        {
            get => _songVolume;
            set
            {
                _songVolume = value;
                _player.SetVolume(value / 10.0f);
                RaisePropertyChanged();
            }
        }

        // Commands

        public RelayCommand WindowClosingCommand { get; private set; }
        public RelayCommand PlaybackCommand { get; private set; }
        public RelayCommand NextSongCommand { get; private set; }
        public RelayCommand PrevSongCommand { get; private set; }
        public RelayCommand StopSongCommand { get; private set; }
        public RelayCommand SeekPlaybackPositionCommand { get; private set; }
        public RelayCommand StartDragCommand { get; private set; }
        public RelayCommand StopDragCommand { get; private set; }
        public RelayCommand UpdateRateCommand { get; private set; }
        public RelayCommand UpdateSongRateCommand { get; private set; }
        public RelayCommand ShowLyricsCommand { get; private set; }
        public RelayCommand ShowYoutubeCommand { get; private set; }
        public RelayCommand SwitchViewModeCommand { get; private set; }

        // This variable is set to true while the slider thumb is being dragged
        // (Binding the event "Thumb.DragCompleted" in XAML to any command 
        //  doesn't work for some reason ¯\_(ツ)_/¯)
        private bool _isDragged;

        private bool _isLoading;

        // optional reference to Performer's view (for UI update)
        public PerformerViewModel Performer { get; set; }


        public AlbumPlaybackViewModel(IAlbumService albumService,
                                      IAudioPlayer player,
                                      IRateCalculator rateCalculator,
                                      ILyricsWebLoader lyricsWebLoader,
                                      IVideoLinkWebLoader videoLinkWebLoader)
        {
            Guard.AgainstNull(albumService);
            Guard.AgainstNull(player);
            Guard.AgainstNull(rateCalculator);
            Guard.AgainstNull(lyricsWebLoader);
            Guard.AgainstNull(videoLinkWebLoader);

            _albumService = albumService;
            _player = player;
            _rateCalculator = rateCalculator;
            _lyricsWebLoader = lyricsWebLoader;
            _videoLinkWebLoader = videoLinkWebLoader;

            // setting up commands
            PlaybackCommand = new RelayCommand(PlaybackSongAction);
            NextSongCommand = new RelayCommand(NextSong);
            PrevSongCommand = new RelayCommand(PrevSong);
            StopSongCommand = new RelayCommand(StopSong);
            UpdateRateCommand = new RelayCommand(UpdateRate);
            UpdateSongRateCommand = new RelayCommand(UpdateSongRate);
            SeekPlaybackPositionCommand = new RelayCommand(SeekPlaybackPosition);
            ShowLyricsCommand = new RelayCommand(ShowLyrics);
            ShowYoutubeCommand = new RelayCommand(ShowYoutube);
            SwitchViewModeCommand = new RelayCommand(SwitchViewMode);

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

            var songs = await _albumService.GetAlbumSongsAsync(Album.Id);

            Songs = Mapper.Map<ObservableCollection<Song>>(songs);

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
                if (SelectedSong == Songs.Last())
                {
                    SelectedSong = null;
                }
                else
                {
                    NextSong();
                }
            }

            PlaybackPercentage = _player.PlayedTimePercent * 10.0;
        }

        private void NextSong()
        {
            if (SelectedSong == Songs.Last())
            {
                return;
            }

            IsLyricsVisible = Visibility.Collapsed;

            SelectedSong = Songs.SkipWhile(s => s != SelectedSong)
                                .Skip(1)
                                .FirstOrDefault();
            PlaySong();
        }

        private void PrevSong()
        {
            if (SelectedSong == Songs.First())
            {
                return;
            }

            SelectedSong = Songs.Reverse()
                                .SkipWhile(s => s != SelectedSong)
                                .Skip(1)
                                .FirstOrDefault();
            PlaySong();
        }

        private void StopSong()
        {
            _player.Stop();
            SelectedSong = null;
            PlaybackSymbol = SymbolPlay;
        }


        #region Song playback functions

        private void PlaySong()
        {
            PlaybackPercentage = 0.0;
            TimePlayed = "0:00";

            if (SelectedSong == null)
            {
                PlaybackSymbol = SymbolPlay;
                return;
            }

            if (_player.SongPlaybackState != PlaybackState.Stop)
            {
                _player.Stop();
            }

            var songfile = FileLocator.FindSongPath(SelectedSong);

            try
            {
                _player.Play(songfile);
                PlaybackSymbol = SymbolPause;
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
                    PlaybackSymbol = SymbolPlay;
                    break;
                case PlaybackState.Pause:
                    _player.Resume();
                    PlaybackSymbol = SymbolPause;
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
        private async void UpdateRate()
        {
            Performer?.UpdateAlbumCollectionRate(_rateCalculator);
            await _albumService.UpdateAlbumRateAsync(Album.Id, Album.Rate);
        }

        private async void UpdateSongRate()
        {
            try
            {
                await _albumService.UpdateSongRateAsync(SelectedSong.Id, SelectedSong.Rate);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        private void SwitchViewMode()
        {
            WindowOpacity = WindowOpacity > 0.25 ? 0.25 : 1;
            IsTracklistVisible = IsTracklistVisible == Visibility.Hidden ? Visibility.Visible : Visibility.Hidden;
            
            if (IsTracklistVisible == Visibility.Visible)
            {
                IsLyricsVisible = Visibility.Collapsed;
            }
        }

        private async void ShowLyrics()
        {
            if (IsLyricsVisible == Visibility.Visible)
            {
                IsLyricsVisible = Visibility.Collapsed;
            }
            else
            {
                var performerName = SelectedSong.Album.Performer.Name;

                var lyricsText = await _lyricsWebLoader.LoadLyricsAsync(performerName, SelectedSong.Name);

                Lyrics = $"{SelectedSong.Name.ToUpperInvariant()}\r\n\r\n{lyricsText}";
                IsLyricsVisible = Visibility.Visible;
                SwitchViewMode();
            }
        }
        
        private async void ShowYoutube()
        {
            var performer = SelectedSong.Album.Performer.Name;
            var song = SelectedSong.Name;

            var videosViewModel = new VideosViewModel 
            {
                Title = $"{performer} - {song}"
            };

            var videosWindow = new VideosWindow 
            {
                DataContext = videosViewModel
            };

            videosWindow.Show();

            videosViewModel.VideoLinks = await _videoLinkWebLoader.LoadVideoLinksAsync(performer, song);
        }
    }
}
