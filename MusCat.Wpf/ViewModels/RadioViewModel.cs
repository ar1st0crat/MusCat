using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Util;
using MusCat.Infrastructure.Services;
using MusCat.Util;
using MusCat.ViewModels.Entities;

namespace MusCat.ViewModels
{
    class RadioViewModel : ViewModelBase
    {
        /// <summary>
        /// Radio Station
        /// </summary>
        private readonly IRadioService _radio;

        /// <summary>
        /// Audio player
        /// </summary>
        private readonly IAudioPlayer _player;
        private bool _isStopped;

        // Bitmaps for playback buttons
        private static readonly BitmapImage ImagePlay = App.Current.Resources["ImagePlayButton"] as BitmapImage;
        private static readonly BitmapImage ImagePause = App.Current.Resources["ImagePauseButton"] as BitmapImage;
        
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

        private float _songVolume = 5.0f;
        public float SongVolume
        {
            get { return _songVolume; }
            set
            {
                _songVolume = value;
                _player.SetVolume(value / 10.0f);
                RaisePropertyChanged();
            }
        }

        public Song PreviousSong => _radio.PrevSong;
        public Song CurrentSong => _radio.CurrentSong;
        public Song NextSong => _radio.NextSong;
        public ObservableCollection<Song> RadioArchive => new ObservableCollection<Song>(_radio.SongArchive);
        public ObservableCollection<Song> RadioUpcoming => new ObservableCollection<Song>(_radio.UpcomingSongs);

        public Action<AlbumViewModel> ShowAlbum { get; set; }

        #region commands

        public ICommand PlaybackCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PreviousSongCommand { get; private set; }
        public ICommand NextSongCommand { get; private set; }
        public ICommand ChangeSongCommand { get; private set; }
        public ICommand RemoveSongCommand { get; private set; }
        public ICommand ShowAlbumCommand { get; private set; }
        public ICommand WindowClosingCommand { get; private set; }

        #endregion


        public RadioViewModel(IRadioService radio, IAudioPlayer player)
        {
            Guard.AgainstNull(radio);
            Guard.AgainstNull(player);

            _radio = radio;
            _player = player;

            // ===================== setting up all commands ============================

            PlaybackCommand = new RelayCommand(SongPlaybackAction);
            ShowAlbumCommand = new RelayCommand(ViewAlbumContainingCurrentSong);

            PreviousSongCommand = new RelayCommand(async () =>
            {
                await _radio.MoveToPrevSongAsync();
                PlayCurrentSong();
                UpdateSongPanels();
            });

            NextSongCommand = new RelayCommand(async () =>
            {
                await _radio.MoveToNextSongAsync();
                PlayCurrentSong();
                UpdateSongPanels();
            });
            
            ChangeSongCommand = new RelayCommand(async id =>
            {
                await _radio.ChangeSongAsync((long)id);
                UpdateSongPanels();
            });

            RemoveSongCommand = new RelayCommand(async id =>
            {
                await _radio.RemoveSongAsync((long)id);
                UpdateSongPanels();
            });

            StopCommand = new RelayCommand(() =>
            {
                _player.Stop();
                PlaybackImage = ImagePlay;
            });

            WindowClosingCommand = new RelayCommand(() =>
            {
                _isStopped = true;  // Stop radio when the window is closing 
                _player.Close();    // to avoid a memory leak
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
            PlaybackImage = ImagePause;
        }

        /// <summary>
        /// Method starts radio service
        /// </summary>
        public void StartRadio()
        {
            PlayCurrentSong();

            // There's one general task associated with the radio
            // whose purpose is to play whatever active song in the background thread
            Task.Run(async () =>
            {
                while (!_isStopped)
                {
                    await Task.Delay(1000);

                    if (_player.IsStopped && !_player.IsStoppedManually)
                    {
                        await _radio.MoveToNextSongAsync();
                        PlayCurrentSong();
                        UpdateSongPanels();
                    }
                }
            });
        }

        /// <summary>
        /// Play song currently active in the radio
        /// </summary>
        public void PlayCurrentSong()
        {
            if (_player.SongPlaybackState != PlaybackState.Stop)
            {
                _player.Stop();
            }

            var fileSong = FileLocator.FindSongPath(CurrentSong);

            try
            {
                _player.Play(fileSong);
            }
            //catch (InvalidOperationException)
            //{
            //     some multi-threading issue in debug mode
            //}
            catch (Exception)
            {
                _radio.MoveToNextSong();
            }
        }

        private void SongPlaybackAction()
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
                case PlaybackState.Stop:
                    PlayCurrentSong();
                    PlaybackImage = ImagePause;
                    break;
            }
        }

        private void ViewAlbumContainingCurrentSong()
        {
            var album = Mapper.Map<AlbumViewModel>(_radio.CurrentSong.Album);

            ShowAlbum?.Invoke(album);
        }
    }
}
