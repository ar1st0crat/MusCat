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

        private void ShowSongs()
        {
            if (songs[0] != null)
            {
                ((DockPanel)this.prevSongPanel.FindName("prevSong")).DataContext = songs[0];
                this.prevSongPanel.DataContext = songs[0].Albums;
                this.prevImage.DataContext = songs[0].Albums;
            }

            ((DockPanel)this.curSongPanel.FindName("curSong")).DataContext = songs[1];
            this.curImage.DataContext = songs[1].Albums;
            this.curSongPanel.DataContext = songs[1].Albums;

            ((DockPanel)this.nextSongPanel.FindName("nextSong")).DataContext = songs[2];
            this.nextSongPanel.DataContext = songs[2].Albums;
            this.nextImage.DataContext = songs[2].Albums;


            StartPlayingCurrentSong();
        }


        private Songs SelectRandomSong()
        {
            Songs song = null;

            using (var context = new MusCatEntities())
            {
                Random songSelector = new Random();

                var maxSID = context.Songs.Max( s => s.SID );
                var songNo = songSelector.Next() % maxSID;
                var selectedsongs = from s in context.Songs
                       where s.SID >= songNo
                       select s;

                song = selectedsongs.First();
                song.Albums = (from a in context.Albums
                               where a.AID == song.AID
                               select a).
                               First();
                song.Albums.Performers = (from p in context.Performers 
                                          where p.PID == song.Albums.Performers.PID 
                                          select p).
                                          First();
            }

            return song;
        }


        public RadioPlayerWindow()
        {
            InitializeComponent();

            songs.Add( null );

            var s1 = SelectRandomSong();
            ((DockPanel)this.curSongPanel.FindName("curSong")).DataContext = s1;
            this.curSongPanel.DataContext = s1;
            this.curImage.DataContext = s1.Albums;
            songs.Add( s1 );

            var s2 = SelectRandomSong();
            this.nextImage.DataContext = s2.Albums;
            this.nextSongPanel.DataContext = s2;
            songs.Add(s2);

            ShowSongs();
        }

        private void NextMouseDown(object sender, MouseButtonEventArgs e)
        {
            songs.RemoveAt(0);

            var s = SelectRandomSong();
            this.nextImage.DataContext = s.Albums;
            this.nextSongPanel.DataContext = s;
            songs.Add(s);

            ShowSongs();
        }

        private void PrevMouseDown(object sender, MouseButtonEventArgs e)
        {
            songs.RemoveAt(2);
            songs.Insert(0, null );

            ShowSongs();
        }

        private void RadioPlayerKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Right)
                return;

            songs.RemoveAt(0);

            var s = SelectRandomSong();
            this.nextImage.DataContext = s.Albums;
            songs.Add(s);

            ShowSongs();
        }



        MediaElement mp3song = new MediaElement();
        bool bPlaying = false;

        BitmapImage imagePlay = new BitmapImage(new Uri(@"Images\play.png", UriKind.Relative));
        BitmapImage imagePause = new BitmapImage(new Uri(@"Images\pause.png", UriKind.Relative));


        private void StartPlayingCurrentSong()
        {
            if (bPlaying)
                mp3song.Stop();

            string pathDir = @"F:\" +
                                    songs[1].Albums.Performers.Performer[0] +
                                    System.IO.Path.DirectorySeparatorChar +
                                    songs[1].Albums.Performers.Performer +
                                    System.IO.Path.DirectorySeparatorChar +
                                    songs[1].Albums.AYear + " - " + songs[1].Albums.Album;
            
            if (!Directory.Exists(pathDir) || songs[1].SN >= Directory.GetFiles( pathDir ).Length )
            {
                NextMouseDown(this, null);
            }
            else
            {
                //MessageBox.Show( pathDir );
                //MessageBox.Show(Directory.GetFiles(pathDir)[songs[1].SN - 1]);

                mp3song.Source = new Uri(Directory.GetFiles(pathDir)[songs[1].SN - 1] );
                mp3song.LoadedBehavior = MediaState.Manual;
                mp3song.UnloadedBehavior = MediaState.Manual;

                mp3song.MediaEnded += mp3song_MediaEnded;

                mp3song.Play();
                bPlaying = true;
            }
        }

        private void PausePlayMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (bPlaying)
            {
                mp3song.Pause();
                bPlaying = false;
                this.playback.Source = imagePlay;
            }
            else
            {
                mp3song.Play();
                bPlaying = true;
                this.playback.Source = imagePause;
            }
        }

        private void mp3song_MediaEnded(object sender, RoutedEventArgs e)
        {
            songs.RemoveAt(0);

            var s = SelectRandomSong();
            this.nextImage.DataContext = s.Albums;
            this.nextSongPanel.DataContext = s;
            songs.Add(s);

            ShowSongs();
        }

        private void StopMouseDown(object sender, MouseButtonEventArgs e)
        {
            mp3song.Stop();
        }

        private void SliderVolumeChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            mp3song.Volume = this.VolumeSlider.Value / 10.0;
        }
    }
}
