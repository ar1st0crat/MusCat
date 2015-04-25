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
    
    /// <summary>
    /// Interaction logic for RadioPlayerWindow.xaml
    /// </summary>
    public partial class RadioPlayerWindow : Window
    {
        List<Songs> songs = new List<Songs>();
        int nCurrentSong = 1;

        BitmapImage imagePlay = new BitmapImage(new Uri(@"Images\play.png", UriKind.Relative));
        BitmapImage imagePause = new BitmapImage(new Uri(@"Images\pause.png", UriKind.Relative));

        WaveOut waveOut = new WaveOut();
        
        bool bPlaying = false;


        private void UpdateSongs()
        {
            if (songs[nCurrentSong - 1].SID != -1 )
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


            StartPlayingCurrentSong();
        }


        private Songs SelectRandomSong()
        {
            Songs song = null;

            using (var context = new MusCatEntities())
            {
                Random songSelector = new Random();

                var maxSID = context.Songs.Max( s => s.SID );

                do
                {
                    var songNo = songSelector.Next() % maxSID;
                    var selectedsongs = from s in context.Songs
                                        where s.SID >= songNo
                                        select s;

                    if (this.ShortSongs.IsChecked.Value)
                        selectedsongs = selectedsongs.Where(s => s.STime.Length <= 4 && s.STime.CompareTo("3:00") < 0);

                    song = selectedsongs.First();
                    song.Albums = (from a in context.Albums
                                   where a.AID == song.AID
                                   select a).First();

                    song.Albums.Performers = (from p in context.Performers
                                              where p.PID == song.Albums.Performers.PID
                                              select p).First();
                }
                while (songs.Where(s => s.SID == song.SID).Count() > 0); // && while exists file && );
            }

            return song;
        }


        private void NextSong()
        {
            nCurrentSong++;

            var s = SelectRandomSong();
            this.nextImage.DataContext = s.Albums;
            this.nextSongPanel.DataContext = s;
            songs.Add(s);

            UpdateSongs();
        }
        

        public RadioPlayerWindow()
        {
            InitializeComponent();

            songs.Add(new Songs { SID = -1, AID = -1, PID = -1, Albums = null, SRate = 0, SName = "", STime = "", SInfo ="", SN = 0 } );

            var s1 = SelectRandomSong();
            ((DockPanel)this.curSongPanel.FindName("curSong")).DataContext = s1;
            this.curSongPanel.DataContext = s1;
            this.curImage.DataContext = s1.Albums;
            songs.Add( s1 );

            var s2 = SelectRandomSong();
            this.nextImage.DataContext = s2.Albums;
            this.nextSongPanel.DataContext = s2;
            songs.Add(s2);

            UpdateSongs();
        }

        private void NextMouseDown(object sender, MouseButtonEventArgs e)
        {
            NextSong();
        }

        private void PrevMouseDown(object sender, MouseButtonEventArgs e)
        {
            if ( nCurrentSong > 1 )
                nCurrentSong--;

            UpdateSongs();
        }

        private void RadioPlayerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Right)
                NextSong();
            else if (e.Key == Key.Left)
                PrevMouseDown( this, null );
        }


        private string FindSongPath()
        {
            List<string> pathlist = new List<string>();
            pathlist.Add( @"F:\" );
            pathlist.Add( @"G:\" );
            pathlist.Add( @"G:\Other\" );

            foreach (var rootpath in pathlist)
            {
                string pathDir = rootpath +
                                    songs[nCurrentSong].Albums.Performers.Performer[0] +
                                    System.IO.Path.DirectorySeparatorChar +
                                    songs[nCurrentSong].Albums.Performers.Performer +
                                    System.IO.Path.DirectorySeparatorChar;

                if (Directory.Exists(pathDir))
                {
                    // TODO:
                    // check if song name contains punctuation and eliminate it! Normalize string - remove spaces and punctuation!
                    //
                    string[] dirs = Directory.GetDirectories(pathDir);
                    var neededDirs = dirs.Where(d => d.Contains(songs[nCurrentSong].Albums.AYear.ToString()) && d.Contains(songs[nCurrentSong].Albums.Album));

                    if (neededDirs.Count() > 0)
                        return neededDirs.First();
                }
            }

            return "";
        }

        private void StartPlayingCurrentSong()
        {
            if (bPlaying)
            {
                waveOut.Stop();
                waveOut.Dispose();
            }

            string pathDir = FindSongPath();
            
            if ( pathDir == "" )
            {
                NextMouseDown(this, null);
            }
            else
            {
                Mp3FileReader mp3Reader = new Mp3FileReader( Directory.GetFiles(pathDir)[songs[nCurrentSong].SN - 1] );

                waveOut = new WaveOut();
                waveOut.Init(mp3Reader);
                waveOut.Play();
                waveOut.PlaybackStopped += SongPlaybackStopped;
                
                bPlaying = true;
            }
        }

        private void SongPlaybackStopped(object sender, StoppedEventArgs e)
        {
            waveOut.Stop();
            bPlaying = false;
            NextSong();
        }

        private void PausePlayMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (bPlaying)
            {
                waveOut.Pause();
                bPlaying = false;
                this.playback.Source = imagePlay;
            }
            else
            {
                waveOut.Play();
                bPlaying = true;
                this.playback.Source = imagePause;
            }
        }


        private void StopMouseDown(object sender, MouseButtonEventArgs e)
        {
            waveOut.Stop();
            bPlaying = false;
            this.playback.Source = imagePlay;
        }


        private void SliderVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            waveOut.Volume = (float)(this.VolumeSlider.Value / 10.0);
        }
    }
}
