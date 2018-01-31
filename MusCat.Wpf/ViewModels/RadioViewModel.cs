using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AutoMapper;
using MusCat.Core.Entities;
using MusCat.Core.Interfaces;
using MusCat.Core.Util;
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
                _radio.Player.SetVolume(value / 10.0f);
                RaisePropertyChanged();
            }
        }

        public Song PreviousSong => _radio.PrevSong;
        public Song CurrentSong => _radio.CurrentSong;
        public Song NextSong => _radio.NextSong;
        public ObservableCollection<Song> RadioArchive => new ObservableCollection<Song>(_radio.SongArchive);
        public ObservableCollection<Song> RadioUpcoming => new ObservableCollection<Song>(_radio.UpcomingSongs);

        public Action<AlbumViewModel> ShowAlbum { get; set; }

        // commands
        public ICommand PlaybackCommand { get; private set; }
        public ICommand StopCommand { get; private set; }
        public ICommand PreviousSongCommand { get; private set; }
        public ICommand NextSongCommand { get; private set; }
        public ICommand ChangeSongCommand { get; private set; }
        public ICommand RemoveSongCommand { get; private set; }
        public ICommand ShowAlbumCommand { get; private set; }
        public ICommand WindowClosingCommand { get; private set; }


        public RadioViewModel(IRadioService radio)
        {
            Guard.AgainstNull(radio);
            _radio = radio;

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

            // Stop radio when the window is closing to avoid a memory leak
            // (it will call Close() for media player)
            WindowClosingCommand = new RelayCommand(() =>
            {
                _radio.Stop();
            });

            // ===========================================================================

            _radio.Update = UpdateSongs;

            if (_radio.UpcomingSongs.Any())
            {
                _radio.Start();
            }
            else
            {
                _radio.MakeSonglistAsync().ContinueWith(task =>
                {
                    UpdateSongs();
                    _radio.Start();
                });
            }
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
                    _radio.PlayCurrentSong();
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
