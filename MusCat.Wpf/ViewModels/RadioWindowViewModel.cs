using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces.Audio;
using MusCat.Core.Interfaces.Radio;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.ViewModels.Entities;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MusCat.ViewModels
{
    class RadioWindowViewModel : BindableBase, IDialogAware
    {
        private readonly IDialogService _dialogService;
        private readonly IRadioService _radio;
        private readonly IAudioPlayer _player;

        private bool _isStopped;
        private float _songVolume = 5.0f;

        private string _playbackSymbol = MdlConstants.SymbolPause;

        public float SongVolume
        {
            get { return _songVolume; }
            set { SetProperty(ref _songVolume, value); _player.SetVolume(value / 10.0f); }
        }

        public string PlaybackSymbol
        {
            get { return _playbackSymbol; }
            set { SetProperty(ref _playbackSymbol, value); }
        }

        public Song SelectedUpcomingSong { get; set; }

        public RadioSongViewModel PreviousSong => Mapper.Map<RadioSongViewModel>(_radio.PrevSong);
        public RadioSongViewModel CurrentSong => Mapper.Map<RadioSongViewModel>(_radio.CurrentSong);
        public RadioSongViewModel NextSong => Mapper.Map<RadioSongViewModel>(_radio.NextSong);

        public ObservableCollection<Song> RadioArchive => new ObservableCollection<Song>(_radio.SongArchive);
        public ObservableCollection<Song> RadioUpcoming => new ObservableCollection<Song>(_radio.UpcomingSongs);

        public Action<AlbumViewModel> ShowAlbum { get; set; }

        #region commands

        public DelegateCommand PlaybackCommand { get; }
        public DelegateCommand StopCommand { get;  }
        public DelegateCommand PreviousSongCommand { get;  }
        public DelegateCommand NextSongCommand { get;  }
        public DelegateCommand ChangeSongCommand { get;  }
        public DelegateCommand RemoveSongCommand { get;  }
        public DelegateCommand ShowAlbumCommand { get; }

        #endregion


        public RadioWindowViewModel(IRadioService radio, IAudioPlayer player, IDialogService dialogService)
        {
            Guard.AgainstNull(radio);
            Guard.AgainstNull(player);
            Guard.AgainstNull(dialogService);

            _radio = radio;
            _player = player;
            _dialogService = dialogService;

            // ===================== setting up all commands ============================

            PlaybackCommand = new DelegateCommand(SongPlaybackAction);
            ShowAlbumCommand = new DelegateCommand(ViewAlbum);

            PreviousSongCommand = new DelegateCommand(async () =>
            {
                await _radio.MoveToPrevSongAsync();
                UpdateSongPanels();
                await PlayCurrentSong();
            });

            NextSongCommand = new DelegateCommand(async () =>
            {
                await _radio.MoveToNextSongAsync();
                UpdateSongPanels();
                await PlayCurrentSong();
            });
            
            ChangeSongCommand = new DelegateCommand(async () =>
            {
                await _radio.ChangeSongAsync(SelectedUpcomingSong.Id);
                UpdateSongPanels();
            });

            RemoveSongCommand = new DelegateCommand(async () =>
            {
                await _radio.RemoveSongAsync(SelectedUpcomingSong.Id);
                UpdateSongPanels();
            });

            StopCommand = new DelegateCommand(() =>
            {
                _player.Stop();
                PlaybackSymbol = MdlConstants.SymbolPlay;
            });

            // ===========================================================================

            if (_radio.UpcomingSongs.Any())
            {
                StartRadio();
            }
            else
            {
                _radio.MakeSonglistAsync().ContinueWith(task =>
                {
                    UpdateSongPanels();
                    StartRadio();
                });
            }
        }

        /// <summary>
        /// The method updates three properties (previous, next and currently played songs)
        /// </summary>
        private void UpdateSongPanels()
        {
            RaisePropertyChanged("PreviousSong");
            RaisePropertyChanged("CurrentSong");
            RaisePropertyChanged("NextSong");
            RaisePropertyChanged("RadioArchive");
            RaisePropertyChanged("RadioUpcoming");
            RaisePropertyChanged("Title");

            PlaybackSymbol = MdlConstants.SymbolPause;
        }

        /// <summary>
        /// Method moves the song in the list of upcoming songs 
        /// at certain position to desired position
        /// </summary>
        /// <param name="from">Source index</param>
        /// <param name="to">Target index</param>
        public void MoveUpcomingSong(int from, int to)
        {
            _radio.MoveUpcomingSong(from, to);

            RaisePropertyChanged("RadioUpcoming");

            if (from == 0 || to == 0)
            {
                RaisePropertyChanged("NextSong");
            }
        }

        /// <summary>
        /// Method starts radio service
        /// </summary>
        public void StartRadio()
        {
            PlayCurrentSong();

            // There's one general task associated with the radio
            // whose purpose is to play whatever active song in the background thread
            Task.Factory.StartNew(async () =>
            {
                while (!_isStopped)
                {
                    await Task.Delay(1000);

                    if (_player.IsStopped && !_player.IsStoppedManually)
                    {
                        await _radio.MoveToNextSongAsync();
                        await PlayCurrentSong();
                        UpdateSongPanels();
                    }
                }
            }, TaskCreationOptions.LongRunning);
        }

        /// <summary>
        /// Play song currently active in the radio
        /// </summary>
        public async Task PlayCurrentSong()
        {
            if (_player.SongPlaybackState != PlaybackState.Stop)
            {
                _player.Stop();
            }

            var songpath = FileLocator.FindSongPath(_radio.CurrentSong);

            try
            {
                _player.Play(songpath);
            }
            catch (Exception)
            {
                await _radio.MoveToNextSongAsync();
            }
        }

        private void SongPlaybackAction()
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
                case PlaybackState.Stop:
                    PlayCurrentSong();
                    PlaybackSymbol = MdlConstants.SymbolPause;
                    break;
            }
        }

        private void ViewAlbum()
        {
            var album = Mapper.Map<AlbumViewModel>(_radio.CurrentSong.Album);

            var parameters = new DialogParameters
            {
                { "album", album }
            };

            _dialogService.Show("AlbumWindow", parameters, null);
        }


        #region IDialogAware implementation

        public string Title => $"MusCat Radio: {CurrentSong.Album.Performer.Name} - {CurrentSong.Name}";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogOpened(IDialogParameters parameters)
        {
        }

        public void OnDialogClosed()
        {
            _isStopped = true;  // Stop radio
            _player.Close();    // to avoid a memory leak

            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
        }

        #endregion
    }
}
