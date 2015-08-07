using MusCatalog.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;


namespace MusCatalog.View
{
    /// <summary>
    /// Class for interaction logic for RadioPlayerWindow.xaml
    /// 
    /// TODO: Refactor! Extract class RadioPlayer
    /// 
    /// </summary>
    public partial class RadioPlayerWindow : Window
    {
        // the list of recently played songs
        List<Songs> songs = new List<Songs>();
        int nCurrentSong = 0;

        // the number of recently played songs which we're tracking
        const int MAX_SONGS_IN_ARCHIVE = 25;

        // bitmaps for playback buttons
        BitmapImage imagePlay = App.Current.Resources[ "ImagePlayButton" ] as BitmapImage;
        BitmapImage imagePause = App.Current.Resources[ "ImagePauseButton" ] as BitmapImage;

        // Audio player
        MusCatPlayer player = new MusCatPlayer();


        /// <summary>
        /// RadioPlayerWindow constructor
        /// </summary>
        public RadioPlayerWindow()
        {
            InitializeComponent();

            // we add two songs to the playlist right away:
            // 1. The song for current playback
            // 2. The upcoming song

            var currentSong = SelectRandomSong();
            ((DockPanel)this.curSongPanel.FindName("curSong")).DataContext = currentSong;
            this.curSongPanel.DataContext = currentSong;
            this.curImage.DataContext = currentSong.Albums;
            songs.Add( currentSong );

            var nextSong = SelectRandomSong();
            this.nextImage.DataContext = nextSong.Albums;
            this.nextSongPanel.DataContext = nextSong;
            songs.Add( nextSong );

            // Update MVVM GUI song elements
            UpdateGUISongs();

            // start playing the first song right away
            StartPlayingCurrentSong();
        }


        /// <summary>
        /// The method correctly sets data contexts for song slots in the main window
        /// </summary>
        private void UpdateGUISongs()
        {
            if ( nCurrentSong > 0 )
            {
                ((DockPanel)this.prevSongPanel.FindName("prevSong")).DataContext = songs[nCurrentSong - 1];
                this.prevSongPanel.DataContext = songs[nCurrentSong - 1].Albums;
                this.prevImage.DataContext = songs[nCurrentSong - 1].Albums;
            }

            ((DockPanel)this.curSongPanel.FindName("curSong")).DataContext = songs[nCurrentSong];
            this.curImage.DataContext = songs[nCurrentSong].Albums;
            this.curSongPanel.DataContext = songs[nCurrentSong].Albums;

            ((DockPanel)this.nextSongPanel.FindName("nextSong")).DataContext = songs[nCurrentSong + 1];
            this.nextSongPanel.DataContext = songs[nCurrentSong + 1].Albums;
            this.nextImage.DataContext = songs[nCurrentSong + 1].Albums;

            this.Archive.ItemsSource = songs;
            this.Archive.Items.Refresh();
        }


        /// <summary>
        /// The method selects random song from a local database.
        /// The song is guaranteed to be present in user's file system
        /// </summary>
        /// <returns>Songs object selected randomly from the database</returns>
        private Songs SelectRandomSong()
        {
            Songs song = null;

            using (var context = new MusCatEntities())
            {
                Random songSelector = new Random();

                // find out the maximum song ID in the database
                var maxSID = context.Songs.Max( s => s.SID );

                // we keep select a song randomly until the song file is actually present in our file system
                // and while it isn't present in our archive of recently played songs
                do
                {
                    IQueryable<Songs> selectedsongs;
                    do
                    {
                        // generate random song ID
                        var songNo = songSelector.Next() % maxSID;

                        // the problem here is that our generated ID isn't necessarily present in the database
                        // however there will be at least one song with songID that is greater or equal than this ID
                        selectedsongs = (from s in context.Songs
                                             where s.SID >= songNo
                                             select s).Take(1);

                        // if the filter "Short songs is 'on'" we do additional filtering
                        if (this.ShortSongs.IsChecked.Value)
                            selectedsongs = selectedsongs.Where(
                                    s => s.STime.Length <= 4 && s.STime.CompareTo("3:00") < 0);
                    }
                    while (selectedsongs.Count() < 1);


                    // select the first song from the set of selected songs
                    song = selectedsongs.First();

                    // include the corresponding album of our song
                    song.Albums = (from a in context.Albums
                                    where a.AID == song.AID
                                    select a).First();

                    // do the same thing with performer for included album
                    song.Albums.Performers = (from p in context.Performers
                                               where p.PID == song.Albums.Performers.PID
                                               select p).First();
                }
                while (songs.Where(s => s.SID == song.SID).Count() > 0          // true, if the archive already contains this song
                    || MusCatFileLocator.FindSongPath(song) == "");             // true, if the file with this song doesn't exist
            }

            return song;
        }


        /// <summary>
        /// Switch to next song in the playlist
        /// </summary>
        private void NextSong()
        {
            // check if the archive ("songlist story") is full 
            if (songs.Count >= MAX_SONGS_IN_ARCHIVE)
                songs.RemoveAt(0);                      // if we remove the first element then we don't have to increase nCurrentSong
            else
                nCurrentSong++;                         // otherwise - increase nCurrentSong

            // if the next song is the last one in the playlist
            // then we should select new upcoming song and add it to the playlist
            if (nCurrentSong == songs.Count - 1)
            {
                var s = SelectRandomSong();
                this.nextImage.DataContext = s.Albums;
                this.nextSongPanel.DataContext = s;
                songs.Add(s);
            }

            // update GUI
            UpdateGUISongs();

            // after we added new upcoming song we must play the current song
            // (this song has been "upcoming" before we added new song to the playlist)
            StartPlayingCurrentSong();
        }


        /// <summary>
        /// 
        /// </summary>
        private void PrevSong()
        {
            // if the current song isn't the first one in a songlist
            // we can safely decrease the current position in the songlist
            if (nCurrentSong > 0)
                nCurrentSong--;

            // update GUI
            UpdateGUISongs();

            // after switching to previous song we play it right away
            StartPlayingCurrentSong();
        }
                

        /// <summary>
        /// 
        /// </summary>
        private void StartPlayingCurrentSong()
        {
            this.playback.Source = imagePause;

            string fileSong = MusCatFileLocator.FindSongPath(songs[nCurrentSong]);

            try
            {
                player.Play( fileSong, SongPlaybackStopped );
            }
            catch (Exception)
            {
                songs.RemoveAt(nCurrentSong--);
                
                // if the next song is the last one in the playlist
                // then we should select new upcoming song and add it to the playlist
                if (nCurrentSong == songs.Count - 1)
                {
                    var s = SelectRandomSong();
                    this.nextImage.DataContext = s.Albums;
                    this.nextSongPanel.DataContext = s;
                    songs.Add( s );
                }

                NextSong();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongPlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            if ( player.SongPlaybackState != PlaybackState.STOP )
            {
                player.SongPlaybackState = PlaybackState.STOP;
                NextSong();
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PausePlayMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ( player.SongPlaybackState == PlaybackState.PLAY )
            {
                player.Pause();
                this.playback.Source = imagePlay;
            }
            else if ( player.SongPlaybackState == PlaybackState.PAUSE )
            {
                player.Resume();
                this.playback.Source = imagePause;
            }
            else
            {
                StartPlayingCurrentSong();
                this.playback.Source = imagePause;
            }
        }



        private void SliderVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            player.SetVolume( (float)this.VolumeSlider.Value / 10.0f );
        }


        private void StopMouseDown(object sender, MouseButtonEventArgs e)
        {
            player.Stop();
            this.playback.Source = imagePlay;
        }


        private void NextMouseDown(object sender, MouseButtonEventArgs e)
        {
            NextSong();
        }


        private void PrevMouseDown(object sender, MouseButtonEventArgs e)
        {
            PrevSong();
        }


        private void RadioPlayerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
            {
                NextSong();
            }
            else if (e.Key == Key.Left)
            {
                PrevSong();
            }
        }
    }
}
