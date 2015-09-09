using MusCatalog.Model;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using System.Threading;


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
                RaisePropertyChanged( "Album" );
            }
        }

        public ObservableCollection<Song> Songs
        {
            get { return AlbumView.Songs; }
        }

        private Song selectedSong;
        public Song SelectedSong
        {
            get { return selectedSong; }
            set
            {
                selectedSong = value;
                RaisePropertyChanged("SelectedSong");
                PlaySong();
            }
        }

        public int SelectedSongIndex { get; set; }

        // bitmaps for playback buttons
        private static BitmapImage imagePlay = App.Current.TryFindResource("ImagePlayButton") as BitmapImage;
        private static BitmapImage imagePause = App.Current.TryFindResource("ImagePauseButton") as BitmapImage;

        private BitmapImage playbackImage = imagePause;
        public BitmapImage PlaybackImage
        {
            get { return playbackImage; }
            set
            {
                playbackImage = value;
                RaisePropertyChanged( "PlaybackImage" );
            }
        }

        // playback timer to synchronize playback slider with MediaPlayer
        DispatcherTimer playbackTimer = new DispatcherTimer();

        // Audio player
        AudioPlayer player = new AudioPlayer();

        // percentage
        private double playbackPercentage = 0.0;
        public double PlaybackPercentage
        {
            get { return playbackPercentage; }
            set
            {
                playbackPercentage = value;
                RaisePropertyChanged("PlaybackPercentage");
            }
        }

        // commands
        public RelayCommand WindowClosingCommand { get; private set; }
        public RelayCommand PlaybackCommand { get; private set; }
        public RelayCommand SeekPlaybackPositionCommand { get; private set; }
        public RelayCommand StartDragCommand { get; private set; }
        public RelayCommand StopDragCommand { get; private set; }
        public RelayCommand UpdateRateCommand { get; private set; }

        // This variable is set to true while the slider thumb is being dragged
        // (Binding the event "Thumb.DragCompleted" in XAML to some command doesn't work for some reason)
        private bool bDragged = false;

        // Play song in seperate thread
        private Thread playThread;


        // Constructor
        public AlbumPlaybackViewModel(AlbumViewModel viewmodel)
        {
            // setting up commands
            WindowClosingCommand = new RelayCommand( Close );
            PlaybackCommand = new RelayCommand( PlaybackSongAction );
            UpdateRateCommand = new RelayCommand( UpdateRate );
            SeekPlaybackPositionCommand = new RelayCommand( SeekPlaybackPosition );
            // toggle the bDragged variable
            StartDragCommand = new RelayCommand(() => bDragged = true );             
            StopDragCommand = new RelayCommand(() => bDragged = false );
            
            // set main album view model
            AlbumView = viewmodel;

            // setting up timer for songs playback
            playbackTimer.Tick += new EventHandler(PlaybackTimerTick);
            playbackTimer.Interval = TimeSpan.FromSeconds(2);
        }

        #region Song playback functions

        /// <summary>
        /// Playback actions: Play / Pause
        /// </summary>
        public void PlaybackSongAction()
        {
            switch (player.SongPlaybackState)
            {
                case PlaybackState.PLAY:
                    PauseSong();
                    break;
                case PlaybackState.PAUSE:
                    ResumeSong();
                    break;
            }
        }

        private void PlaySong()
        {
            if (player.SongPlaybackState != PlaybackState.STOP)
            {
                StopSong();
            }

            string songfile = FileLocator.FindSongPath(SelectedSong);

            try
            {
                // play file in separate thread
                playThread = new Thread( () => player.Play(songfile, SongPlaybackStopped) );
                playThread.IsBackground = true;
                playThread.Start();
                
                // launch timer for playback slider tracking 
                playbackTimer.Start();
                PlaybackImage = imagePause;
                PlaybackPercentage = 0.0;
            }
            catch (Exception)
            {
                MessageBox.Show("Sorry, song could not be played");
            }
        }

        private void PauseSong()
        {
            player.Pause();
            playbackTimer.Stop();
            PlaybackImage = imagePlay;
        }

        private void ResumeSong()
        {
            player.Resume();
            playbackTimer.Start();
            PlaybackImage = imagePause;
        }

        private void StopSong()
        {
            player.Stop();
        }

        /// <summary>
        /// Handler of an event fired when the song reached the end
        /// </summary>
        private void SongPlaybackStopped(object sender, EventArgs e)
        {
            playThread.Join();
            playbackTimer.Stop();
            PlaybackImage = imagePlay;

            if (player.IsManualStop == true)
            {
                player.IsManualStop = false;
                return;
            }

            // play next song
            if (SelectedSong == Songs.Last())
            {
                return;
            }

            SelectedSong = Songs.SkipWhile(s => s != SelectedSong).Skip(1).FirstOrDefault();
        }

        /// <summary>
        /// Rewind the song to the position specified by PlaybackPercentage (property bound to playback slider's value)
        /// </summary>
        /// <param name="sender">playback slider</param>
        public void SeekPlaybackPosition()
        {
            // if the slider value was changed with timer (not by user) or the song is stopped
            if (!bDragged || player.SongPlaybackState == PlaybackState.STOP)
            {
                // then do nothing
                return;
            }

            player.Seek( PlaybackPercentage / 10.0);
        }

        #endregion

        /// <summary>
        /// Update the PlaybackPercentage property (and slider thumb position as well)
        /// according to current position of Audio Player
        /// </summary>
        private void PlaybackTimerTick(object sender, EventArgs e)
        {
            if (player.SongPlaybackState != PlaybackState.PLAY)
            {
                return;
            }

            PlaybackPercentage = player.TimePercent() * 10.0;
        }

        /// <summary>
        /// Freeze media player when the window is closing to avoid a memory leak
        /// and unsubscribe from the RateUpdated event for the same reason
        /// </summary>
        public void Close()
        {
            if (playThread != null)
            {
                playThread.Join();
            }
            player.Freeze();
        }

        /// <summary>
        /// Just an additional feature of the AlbumWindow:
        /// user can't edit album info except that he/she can update album rate by clicking on the 5-star rate control
        /// </summary>
        public void UpdateRate()
        {
            using (var context = new MusCatEntities())
            {
                context.Entry(Album).State = System.Data.EntityState.Modified;
                context.SaveChanges();

                // raise event to allow for correct updating of MainViewModel
                if (RateUpdated != null)
                {
                    RateUpdated(Album.Performer, null);
                }
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
