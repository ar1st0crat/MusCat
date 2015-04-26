using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MusCatalog
{
    enum PlaybackState
    {
        PLAY,
        PAUSE,
        STOP
    }

    /// <summary>
    /// Class for interaction logic for RadioPlayerWindow.xaml
    /// </summary>
    public partial class RadioPlayerWindow : Window
    {
        // the list of recently played songs
        List<Songs> songs = new List<Songs>();
        int nCurrentSong = 0;

        // the number of recently played songs which we're tracking
        const int MAX_SONGS_IN_ARCHIVE = 25;

        // bitmaps for playback buttons
        BitmapImage imagePlay = new BitmapImage(new Uri(@"Images\play.png", UriKind.Relative));
        BitmapImage imagePause = new BitmapImage(new Uri(@"Images\pause.png", UriKind.Relative));

        // NAudio component for a song playback
        WaveOut waveOut = new WaveOut();
        PlaybackState playbackState = PlaybackState.STOP;


        /// <summary>
        /// RadioPlayerWindow constructor
        /// </summary>
        public RadioPlayerWindow()
        {
            InitializeComponent();

            // we add two songs to the playlist right away:
            // 1. The song for current playback
            // 2. The upcoming song

            var s1 = SelectRandomSong();
            ((DockPanel)this.curSongPanel.FindName("curSong")).DataContext = s1;
            this.curSongPanel.DataContext = s1;
            this.curImage.DataContext = s1.Albums;
            songs.Add(s1);

            var s2 = SelectRandomSong();
            this.nextImage.DataContext = s2.Albums;
            this.nextSongPanel.DataContext = s2;
            songs.Add(s2);

            // Update MVVM GUI song elements
            UpdateGUISongs();
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

            StartPlayingCurrentSong();
        }


        /// <summary>
        /// The method selects random song from a local database. The song is guaranteed to be present in user's file system
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

                        // the problem here is that our generated ID isn't necessarily present in the database$
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
                    || FindSongPath( song ) == "" );                            // true, if the file with this song doesn't exist
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
        }


        /// <summary>
        /// 
        /// </summary>
        private void PrevSong()
        {
            if (nCurrentSong > 0)
                nCurrentSong--;

            UpdateGUISongs();
        }
        
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        string postProcess(string s)
        {
            string res = "";
            foreach (char c in s)
                if (!Char.IsWhiteSpace(c) && !Char.IsPunctuation(c))
                    res += c;

            return res;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="song"></param>
        /// <returns>the actual path of the file with the song if it was found, an empty string otherwise</returns>
        private string FindSongPath( Songs song )
        {
            List<string> pathlist = new List<string>();
            pathlist.Add( @"F:\" );
            pathlist.Add( @"G:\" );
            pathlist.Add( @"G:\Other\" );

            foreach (var rootpath in pathlist)
            {
                string pathDir = rootpath +
                                    song.Albums.Performers.Performer[0] +
                                    System.IO.Path.DirectorySeparatorChar +
                                    song.Albums.Performers.Performer +
                                    System.IO.Path.DirectorySeparatorChar;

                if (Directory.Exists(pathDir))
                {
                    // TODO:
                    // 1) check if song name contains punctuation and eliminate it! Normalize string - remove spaces and punctuation!
                    // 2) check if the album is double!

                    string[] dirs = Directory.GetDirectories(pathDir);
                    var neededDirs = dirs.Where(d => d.Contains(song.Albums.AYear.ToString() ) );// && postProcess(song.Albums.Album) == postProcess(d));

                    if (neededDirs.Count() > 0)
                    {
                        if (song.Albums.Songs.Count(s => s.SID == song.SID) > 1)
                            return neededDirs.AsEnumerable().ElementAt(1);
                        else
                            return neededDirs.First();
                    }
                }
            }

            return "";
        }

        
        /// <summary>
        /// 
        /// </summary>
        private void StartPlayingCurrentSong()
        {
            waveOut.Stop();
            waveOut.Dispose();
            waveOut = null;
  
            string pathDir = FindSongPath( songs[nCurrentSong] );
            
            if ( pathDir == "" )
            {
                NextSong();
            }
            else
            {
                Mp3FileReader mp3Reader = null;

                try
                {
                    mp3Reader = new Mp3FileReader(Directory.GetFiles(pathDir)[songs[nCurrentSong].SN - 1]);
                    {
                        waveOut = new WaveOut();

                        waveOut.Init(mp3Reader);
                        waveOut.Play();
                        waveOut.PlaybackStopped += SongPlaybackStopped;

                        playbackState = PlaybackState.PLAY;
                    }
                }
                catch (Exception ex)
                {
                    songs.RemoveAt(nCurrentSong--);
                    waveOut = new WaveOut();

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

                    NextSong();
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongPlaybackStopped(object sender, StoppedEventArgs e)
        {
            if ( playbackState != PlaybackState.STOP )
            {
                playbackState = PlaybackState.STOP;
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
            if ( playbackState == PlaybackState.PLAY )
            {
                waveOut.Pause();
                playbackState = PlaybackState.PAUSE;
                this.playback.Source = imagePlay;
            }
            else if (playbackState == PlaybackState.PAUSE)
            {
                playbackState = PlaybackState.PLAY;
                waveOut.Resume();
                this.playback.Source = imagePause;
            }
            else
            {
                StartPlayingCurrentSong();
                this.playback.Source = imagePause;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StopMouseDown(object sender, MouseButtonEventArgs e)
        {
            playbackState = PlaybackState.STOP;
            waveOut.Stop();
            this.playback.Source = imagePlay;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SliderVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            waveOut.Volume = (float)(this.VolumeSlider.Value / 10.0);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NextMouseDown(object sender, MouseButtonEventArgs e)
        {
            NextSong();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PrevMouseDown(object sender, MouseButtonEventArgs e)
        {
            PrevSong();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioPlayerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
                NextSong();
            else if (e.Key == Key.Left)
                PrevSong();
        }
    }
}
