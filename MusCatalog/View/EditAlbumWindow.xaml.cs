using MusCatalog.Model;
using System;
using System.Linq;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Collections.Generic;

namespace MusCatalog.View
{
    /// <summary>
    /// Interaction logic for EditAlbumWindow.xaml
    /// </summary>
    public partial class EditAlbumWindow : Window
    {
        private Album album = null;

        // Song list
        List<Song> albumSongs = new List<Song>();

        // bitmaps for stars
        BitmapImage imageStar = App.Current.TryFindResource("ImageStar") as BitmapImage;
        BitmapImage imageHalfStar = App.Current.TryFindResource("ImageHalfStar") as BitmapImage;
        BitmapImage imageEmptyStar = App.Current.TryFindResource("ImageEmptyStar") as BitmapImage;

        // number of the star that was clicked
        byte starPos = 0;


        public EditAlbumWindow( Album a )
        {
            InitializeComponent();

            // save current album in 'album' variable
            if (a != null)
            {
                album = a;
            }
            // or create new empty album with ID = -1
            else
            {
                album = new Album { ID = -1 };
            }

            // load and prepare all songs from the album for further actions
            using (var context = new MusCatEntities())
            {
                albumSongs = context.Songs.Where(s => s.Album.ID == a.ID).ToList();

                foreach (var song in albumSongs)
                {
                    song.Album = album;
                }

                this.rateAlbum.DataContext = a;
                this.AlbumInfoPanel.DataContext = a;
                this.GridSongs.ItemsSource = albumSongs;    
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private void StarMouseMove(object sender, MouseEventArgs e)
        {
            starPos = 0;

            // loop to find out what star has triggered the MouseMove event
            foreach (var elem in this.rateAlbum.Children)
            {
                starPos++;

                if (elem == (Image)sender)
                    break;
            }

            // draw all stars to the left as "full" stars
            for (int i = 0; i < starPos - 1; i++)
            {
                ((Image)this.rateAlbum.Children[i]).Source = imageStar;
            }

            // if the X coordinate of mouse position exceeds the half of a star size
            if (e.GetPosition((Image)sender).X > 25 / 2)
            {
                // then draw full star
                ((Image)sender).Source = imageStar;
            }
            else
            {
                // else draw half of the star
                ((Image)sender).Source = imageHalfStar;
            }

            // rest of the stars are empty
            for (int i = starPos; i < 5; i++)
            {
                ((Image)this.rateAlbum.Children[i]).Source = imageEmptyStar;
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void StarMouseLeave(object sender, MouseEventArgs e)
        {
            starPos = 0;
            for (int i = 0; i < 5; i++)
            {
                ((Image)this.rateAlbum.Children[i]).Source = imageEmptyStar;
            }

            if (album.Rate.HasValue)
            {
                int i = 0;
                for (; i < album.Rate / 2; i++)
                {
                    ((Image)this.rateAlbum.Children[i]).Source = imageStar;
                }

                if (album.Rate.Value % 2 == 1)
                {
                    ((Image)this.rateAlbum.Children[i]).Source = imageHalfStar;
                }
            }
        }


        /// <summary>
        /// When the user clicks on a star, we update the album rate in database
        /// </summary>
        private void StarMouseDown(object sender, MouseButtonEventArgs e)
        {
            album.Rate = (byte)(starPos * 2);

            if (((Image)sender).Source == imageHalfStar)
            {
                album.Rate--;
            }

            // update database
            using (var context = new MusCatEntities())
            {
                context.Entry(album).State = System.Data.EntityState.Modified;
                context.SaveChanges();
            }
        }


        private void ParseMp3Click(object sender, RoutedEventArgs e)
        {

        }

        private void SaveSongClick(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            
            var s = this.GridSongs.SelectedItem as Song;
            if ( s != null )
                MessageBox.Show(b.CommandParameter + " " + s.Name);
            else
                MessageBox.Show(this.GridSongs.SelectedCells[1].Item + "");
        }

        private void DeleteSongClick(object sender, RoutedEventArgs e)
        {
            var b = sender as Button;
            MessageBox.Show(b.CommandParameter + "");
        }

        private void AddSongClick(object sender, RoutedEventArgs e)
        {
            if ( this.GridSongs.Items.Count > 0 && albumSongs.Last().ID == -1 )
            {
                return;
            }

            byte newTrackNo = (byte)(albumSongs.Last().TrackNo + 1);
            albumSongs.Add( new Song { ID = -1, TrackNo = newTrackNo} );
            
            this.GridSongs.ItemsSource = albumSongs;
            this.GridSongs.Items.Refresh();
        }

        private void LoadAlbumImageFromClipboard(object sender, RoutedEventArgs e)
        {
            if (!Clipboard.ContainsImage())
            {
                MessageBox.Show("No image in clipboard!");
                return;
            }

            var filepaths = FileLocator.MakePathImageAlbum( album );
            string filepath = filepaths[0];

            if (filepaths.Count > 1)
            {
                ChoiceWindow choice = new ChoiceWindow();
                choice.SetChoiceList(filepaths);
                choice.ShowDialog();

                if (choice.ChoiceResult == "")
                {
                    return;
                }
                filepath = choice.ChoiceResult;
            }
                
            Directory.CreateDirectory(Path.GetDirectoryName(filepath));

            var image = Clipboard.GetImage();
            try
            {
                // first check if file already exists
                if (File.Exists(filepath))
                {
                    File.Delete(filepath);
                }

                using (var fileStream = new FileStream(filepath, FileMode.CreateNew))
                {
                    BitmapEncoder encoder = new JpegBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);

                    this.AlbumCover.Source = encoder.Frames[0];
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void LoadAlbumImageFromFile(object sender, RoutedEventArgs e)
        {

        }

        private void SaveAlbumInformation(object sender, RoutedEventArgs e)
        {
            using (var context = new MusCatEntities())
            {
                context.Entry(album).State = System.Data.EntityState.Modified;
                context.SaveChanges();
            }
        }

        private void FixNamesClick(object sender, RoutedEventArgs e)
        {

        }

        private void FixTimesClick(object sender, RoutedEventArgs e)
        {

        }
    }
}
