using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using MusCat.Model;
using MusCat.Utils;

namespace MusCat.ViewModel
{
    class AlbumPlaybackViewModel : INotifyPropertyChanged
    {
        public PerformerViewModel Performer { get; set; }
        public AlbumViewModel AlbumView { get; set; }
        
        public Album Album
        {
            get { return AlbumView.Album; }
            set
            {
                AlbumView.Album = value;
                RaisePropertyChanged("Album");
            }
        }

        public string AlbumHeader => string.Format("{0} - {1} ({2})", 
                                                        Album.Performer.Name,
                                                        Album.Name,
                                                        Album.ReleaseYear);

        public ObservableCollection<Song> Songs => AlbumView.Songs;

        private Song _selectedSong;
        public Song SelectedSong
        {
            get { return _selectedSong; }
            set
            {
                _selectedSong = value;
                RaisePropertyChanged("SelectedSong");
                PlaySong();
            }
        }

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
                RaisePropertyChanged("PlaybackImage");
            }
        }

        // Audio player
        private readonly AudioPlayer _player = new AudioPlayer();
        private readonly BackgroundWorker _worker = new BackgroundWorker();
        private bool _isStopped;

        // Song time percentage
        private double _playbackPercentage;
        public double PlaybackPercentage
        {
            get { return _playbackPercentage; }
            set
            {
                _playbackPercentage = value;
                RaisePropertyChanged("PlaybackPercentage");
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


        public AlbumPlaybackViewModel(AlbumViewModel viewmodel)
        {
            // setting up commands
            PlaybackCommand = new RelayCommand(PlaybackSongAction);
            UpdateRateCommand = new RelayCommand(UpdateRate);
            SeekPlaybackPositionCommand = new RelayCommand(SeekPlaybackPosition);

            // StopAndDispose media player when the window is closing to avoid a memory leak
            WindowClosingCommand = new RelayCommand(() =>
            {
                _isStopped = true;
                _player.StopAndDispose();
            });
            
            // toggle the _isDragged variable
            StartDragCommand = new RelayCommand(() => _isDragged = true);             
            StopDragCommand = new RelayCommand(() => _isDragged = false);
            
            // set main album view model
            AlbumView = viewmodel;

            InitPlayerWorker();
        }

        /// <summary>
        /// There's one general background worker associated with the album window
        /// whise purpose is to play selected song in the background thread
        /// </summary>
        private void InitPlayerWorker()
        {
            _worker.DoWork += (o, e) =>
            {
                while (!_isStopped)
                {
                    Task.Delay(1000).Wait();

                    if (SelectedSong != null)
                    {
                        UpdateSong();
                    }
                }
            };

            _worker.RunWorkerAsync();
        }

        private void UpdateSong()
        {
            if (_player.IsStopped() && !_player.IsStoppedManually)
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

            PlaybackPercentage = _player.TimePercent() * 10.0;
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
            // if the slider value was changed with timer (not by user) or the song is stopped
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
            using (var context = new MusCatEntities())
            {
                context.Entry(Album).State = EntityState.Modified;
                context.SaveChanges();

                Performer?.UpdateAlbumCollectionRate();
            }
        }

        #region INotifyPropertyChanged event and method

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
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
