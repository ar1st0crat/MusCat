using AutoMapper;
using MusCat.Application.Interfaces;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Audio;
using MusCat.Core.Interfaces.Networking;
using MusCat.Core.Util;
using MusCat.Events;
using MusCat.Infrastructure.Services;
using MusCat.ViewModels.Entities;
using Prism.Commands;
using Prism.Events;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace MusCat.ViewModels
{
    class AlbumWindowViewModel : BindableBase, IDialogAware
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly IDialogService _dialogService;
        private readonly IAudioPlayer _player;
        private readonly IAlbumService _albumService;
        private readonly ILyricsWebLoader _lyricsWebLoader;
        
        private AlbumViewModel _album;
        public AlbumViewModel Album
        {
            get { return _album; }
            set { SetProperty(ref _album, value); }
        }

        private ObservableCollection<Song> _songs;
        public ObservableCollection<Song> Songs
        {
            get { return _songs; }
            set { SetProperty(ref _songs, value); }
        }

        private string _timePlayed;
        public string TimePlayed
        {
            get { return _timePlayed; }
            set { SetProperty(ref _timePlayed, value); }
        }

        private string _lyrics;
        public string Lyrics
        {
            get { return _lyrics; }
            set { SetProperty(ref _lyrics, value); }
        }

        private Visibility _isLyricsVisible = Visibility.Collapsed;
        public Visibility IsLyricsVisible
        {
            get { return _isLyricsVisible; }
            set { SetProperty(ref _isLyricsVisible, value); }
        }

        private string _playbackSymbol = MdlConstants.SymbolPause;
        public string PlaybackSymbol
        {
            get { return _playbackSymbol; }
            set { SetProperty(ref _playbackSymbol, value); }
        }

        private double _windowOpacity = 0.25;
        public double WindowOpacity
        {
            get { return _windowOpacity; }
            set { SetProperty(ref _windowOpacity, value); }
        }

        private Visibility _isTracklistVisible = Visibility.Visible;
        public Visibility IsTracklistVisible
        {
            get { return _isTracklistVisible; }
            set { SetProperty(ref _isTracklistVisible, value); }
        }

        private Song _selectedSong;
        public Song SelectedSong
        {
            get { return _selectedSong; }
            set
            {
                SetProperty(ref _selectedSong, value);

                IsLyricsVisible = Visibility.Collapsed;

                if (!_isLoading) PlaySong();
            }
        }

        private double _playbackPercentage;
        public double PlaybackPercentage
        {
            get { return _playbackPercentage; }
            set
            {
                SetProperty(ref _playbackPercentage, value);

                TimePlayed = $"{(int)_player.PlayedTime / 60}:{(int)_player.PlayedTime % 60:00}";
            }
        }

        private float _songVolume = 5.0f;
        public float SongVolume
        {
            get { return _songVolume; }
            set
            {
                SetProperty(ref _songVolume, value);

                _player.SetVolume(value / 10.0f);
            }
        }

        private bool _isLoading;
        private bool _isStopped;

        /// <summary>
        /// This variable is set to true while the slider thumb is being dragged
        /// (Binding the event "Thumb.DragCompleted" in XAML to any command 
        ///  doesn't work for some reason ¯\_(ツ)_/¯)
        /// </summary>
        private bool _isDragged;


        // Commands

        public DelegateCommand WindowClosingCommand { get;  }
        public DelegateCommand PlaybackCommand { get; }
        public DelegateCommand NextSongCommand { get; }
        public DelegateCommand PrevSongCommand { get; }
        public DelegateCommand StopSongCommand { get; }
        public DelegateCommand SeekPlaybackPositionCommand { get; }
        public DelegateCommand StartDragCommand { get; }
        public DelegateCommand StopDragCommand { get; }
        public DelegateCommand UpdateRateCommand { get; }
        public DelegateCommand UpdateSongRateCommand { get; }
        public DelegateCommand ShowLyricsCommand { get; }
        public DelegateCommand ShowYoutubeCommand { get; }
        public DelegateCommand SwitchViewModeCommand { get; }

        
        public AlbumWindowViewModel(IEventAggregator eventAggregator,
                                    IDialogService dialogService,
                                    IAlbumService albumService,
                                    IAudioPlayer player,
                                    ILyricsWebLoader lyricsWebLoader)
        {
            Guard.AgainstNull(eventAggregator);
            Guard.AgainstNull(dialogService);
            Guard.AgainstNull(albumService);
            Guard.AgainstNull(player);
            Guard.AgainstNull(lyricsWebLoader);

            _eventAggregator = eventAggregator;
            _dialogService = dialogService;
            _albumService = albumService;
            _player = player;
            _lyricsWebLoader = lyricsWebLoader;

            // setting up commands

            PlaybackCommand = new DelegateCommand(PlaybackSongAction);
            NextSongCommand = new DelegateCommand(NextSong);
            PrevSongCommand = new DelegateCommand(PrevSong);
            StopSongCommand = new DelegateCommand(StopSong);
            UpdateRateCommand = new DelegateCommand(UpdateRate);
            UpdateSongRateCommand = new DelegateCommand(UpdateSongRate);
            SeekPlaybackPositionCommand = new DelegateCommand(SeekPlaybackPosition);
            ShowLyricsCommand = new DelegateCommand(ShowLyrics);
            ShowYoutubeCommand = new DelegateCommand(ShowYoutube);
            SwitchViewModeCommand = new DelegateCommand(SwitchViewMode);

            // toggle the _isDragged variable

            StartDragCommand = new DelegateCommand(() => _isDragged = true);
            StopDragCommand = new DelegateCommand(() => _isDragged = false);
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
            Task.Factory.StartNew(async () =>
            {
                while (!_isStopped)
                {
                    await Task.Delay(1000);

                    if (SelectedSong != null)
                    {
                        UpdateSong();
                    }
                }
            }, TaskCreationOptions.LongRunning);
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
            PlaybackSymbol = MdlConstants.SymbolPlay;
        }


        #region Song playback functions

        private void PlaySong()
        {
            PlaybackPercentage = 0.0;
            TimePlayed = "0:00";

            if (SelectedSong == null)
            {
                PlaybackSymbol = MdlConstants.SymbolPlay;
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
                PlaybackSymbol = MdlConstants.SymbolPause;
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
                    PlaybackSymbol = MdlConstants.SymbolPlay;
                    break;
                case PlaybackState.Pause:
                    _player.Resume();
                    PlaybackSymbol = MdlConstants.SymbolPause;
                    break;
            }
        }

        /// <summary>
        /// Rewind the song to the position specified by PlaybackPercentage
        /// (property bound to playback slider's value)
        /// </summary>
        private void SeekPlaybackPosition()
        {
            // if the slider value was changed with timer (not by user) or the song is stopped

            if (!_isDragged || _player.SongPlaybackState == PlaybackState.Stop)
            {
                return; // then do nothing
            }

            _player.Seek(PlaybackPercentage / 10.0);
        }

        #endregion

        
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

        /// <summary>
        /// Just an additional feature of the AlbumWindow:
        /// user can update album rate
        /// by clicking on the 5-star rate control
        /// </summary>
        private async void UpdateRate()
        {
            _eventAggregator.GetEvent<AlbumRateUpdatedEvent>().Publish(Album);

            await _albumService.UpdateAlbumRateAsync(Album.Id, Album.Rate);
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
        
        private void ShowYoutube()
        {
            var performer = SelectedSong.Album.Performer.Name;
            var song = SelectedSong.Name;

            var parameters = new DialogParameters
            {
                { "performer", performer },
                { "song", song }
            };

            _dialogService.Show("VideosWindow", parameters, null);
        }


        #region IDialogAware implementation

        public string Title => $"{Album.Performer?.Name} - {Album.Name} ({Album.ReleaseYear})";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
            Album = parameters.GetValue<AlbumViewModel>("album");

            LoadSongsAsync();
        }

        public void OnDialogClosed()
        {
            // StopAndDispose media player when the window is closing to avoid a memory leak

            _isStopped = true;
            _player.Close();
        }

        #endregion
    }
}
