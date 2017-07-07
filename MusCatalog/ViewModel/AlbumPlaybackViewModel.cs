using MusCatalog.Model;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.Entity;
using System.Windows;
using System.Windows.Media.Imaging;
using System.Threading;
using MusCatalog.Utils;

namespace MusCatalog.ViewModel
{
    class AlbumPlaybackViewModel : INotifyPropertyChanged
    {
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

        public int SelectedSongIndex { get; set; }

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
        readonly AudioPlayer _player = new AudioPlayer();

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
        // (Binding the event "Thumb.DragCompleted" in XAML to some command doesn't work for some reason)
        private bool _isDragged;


        public AlbumPlaybackViewModel(AlbumViewModel viewmodel)
        {
            // setting up commands
            WindowClosingCommand = new RelayCommand(Close);
            PlaybackCommand = new RelayCommand(PlaybackSongAction);
            UpdateRateCommand = new RelayCommand(UpdateRate);
            SeekPlaybackPositionCommand = new RelayCommand(SeekPlaybackPosition);
            // toggle the _isDragged variable
            StartDragCommand = new RelayCommand(() => _isDragged = true);             
            StopDragCommand = new RelayCommand(() => _isDragged = false);
            
            // set main album view model
            AlbumView = viewmodel;
        }

        #region Song playback functions

        /// <summary>
        /// Playback actions: Play / Pause
        /// </summary>
        public void PlaybackSongAction()
        {
            switch (_player.SongPlaybackState)
            {
                case PlaybackState.Play:
                    PauseSong();
                    break;
                case PlaybackState.Pause:
                    ResumeSong();
                    break;
            }
        }

        private void PlaySong()
        {
            if (_player.SongPlaybackState != PlaybackState.Stop)
            {
                StopSong();
            }

            var songfile = FileLocator.FindSongPath(SelectedSong);

            // play song using BackgroundWorker
            var bw = new BackgroundWorker();
            bw.DoWork += (o, e) =>
            {
                PlaybackPercentage = 0.0;

                try
                {
                    _player.Play(songfile);
                }
                catch (Exception)
                {
                    // if the exception was thrown, show message
                    MessageBox.Show("Sorry, song could not be played");
                    // and manually set STOP flag in order to not proceed to next song
                    _player.IsStoppedManually = true;
                    return;
                }
                
                // loop while song was not stopped (naturally or manually)
                while (!_player.IsStopped())
                {
                    // update the slider value
                    PlaybackPercentage = _player.TimePercent() * 10.0;
                    Thread.Sleep(1000);
                }

                PlaybackImage = ImagePause;
            };

            // when song was stopped... 
            bw.RunWorkerCompleted += (o, e) =>
            {
                // if song was stopped by user then do nothing
                if (_player.IsStoppedManually == true)
                {
                    return;
                }

                // else play next song (if current song is not the last one)
                if (SelectedSong == Songs.Last())
                {
                    return;
                }

                SelectedSong = Songs.SkipWhile(s => s != SelectedSong).Skip(1).FirstOrDefault();
            };

            bw.RunWorkerAsync();
        }

        private void PauseSong()
        {
            _player.Pause();
            PlaybackImage = ImagePlay;
        }

        private void ResumeSong()
        {
            _player.Resume();
            PlaybackImage = ImagePause;
        }

        private void StopSong()
        {
            _player.Stop();
        }

        /// <summary>
        /// Rewind the song to the position specified by PlaybackPercentage (property bound to playback slider's value)
        /// </summary>
        public void SeekPlaybackPosition()
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
        /// StopAndDispose media player when the window is closing to avoid a memory leak
        /// </summary>
        public void Close()
        {
            _player.StopAndDispose();
        }

        /// <summary>
        /// Just an additional feature of the AlbumWindow:
        /// user can't edit album info except that he/she can update album rate by clicking on the 5-star rate control
        /// </summary>
        public void UpdateRate()
        {
            using (var context = new MusCatEntities())
            {
                context.Entry(Album).State = EntityState.Modified;
                context.SaveChanges();

                // raise event to allow for correct updating of MainViewModel
                // (thread-safe way, according to ECMA)
                EventHandler handler;
                lock (this)
                {
                    handler = RateUpdated;
                }
                handler?.Invoke(Album.Performer, EventArgs.Empty);
            }
        }

        public event EventHandler RateUpdated;

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
