﻿using MusCatalog.Model;
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using System.Windows.Media.Imaging;


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


        public AlbumPlaybackViewModel(AlbumViewModel viewmodel)
        {
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
                player.Play(songfile, SongPlaybackStopped);
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
            playbackTimer.Stop();
            PlaybackImage = imagePlay;
        }

        /// <summary>
        /// Handler of an event fired when the song reached the end
        /// </summary>
        private void SongPlaybackStopped(object sender, EventArgs e)
        {
            StopSong();

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
            if (player.SongPlaybackState == PlaybackState.STOP)
            {
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
